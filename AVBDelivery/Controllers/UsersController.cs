using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AVBDelivery.Jobs;
using AVBDelivery.Models;
using AVBDelivery.Models.AmoCrm;
using AVBDelivery.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AVBDelivery.Controllers
{
    public class UsersController : Controller
    {
        UserManager<User> _userManager;
        private AmoCrm _amoCrm;
        private ApplicationContext _context;
        User _User;
        public UsersController(UserManager<User> userManager, ApplicationContext context, AmoCrm amoCrm)
        {
            _userManager = userManager;
            _amoCrm = amoCrm;
            _context = context;
        }

        //public IActionResult Index() => View(_userManager.Users.ToList());
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var model = new UsersViewModel();
            var usersWithClients = new List<UserWithClient>();
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                var userWithClient = new UserWithClient
                {
                    User = user
                };
                var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (contact != null)
                {
                    userWithClient.Contact = contact;
                }
                usersWithClients.Add(userWithClient);
            }
            model.UsersWithClients = usersWithClients;
            return View(model);
        }

        [Authorize(Roles = "admin")]
        public IActionResult Create() => View();

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            await GetUserInfo();
            
            if (_User == null)
            {
                return NotFound();
            }
            var contact = await _context.Contacts.Include(c => c.Organizations).FirstOrDefaultAsync(c => c.UserId == _User.Id);
            if (contact == null)
            {
                return NotFound();
            }
            
            var model = new ProfileViewModel
            {
                Contact = contact,
                PhoneNumber = _User.PhoneNumber,
                Email = _User.Email
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Email = model.Email, UserName = model.Email, PhoneNumber = model.PhoneNumber ?? "", PhoneNumberConfirmed = true, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var contact = new Contact
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = user.Id,
                        Name = model.Name
                    };

                    if (model.IsClient)
                    {
                        
                        var amoContacts = (await _amoCrm.GetContacts()).Embedded.Contacts;
                        var foundedContact = amoContacts
                            .FirstOrDefault(c => c.CustomFieldsValues?
                                .FirstOrDefault(f => f.FieldCode == "PHONE")?.Values?
                                .FirstOrDefault()?.Value == model.PhoneNumber);

                        if (foundedContact != null)
                        {
                            contact.AmoCrmId = foundedContact.Id.ToString();
                            contact.Name = foundedContact.Name;
                        }
                        else
                        {
                            var amoContact = new AmoContact
                            {
                                Name = model.Name,
                                CustomFieldsValues =
                                [
                                    new CustomFieldValues
                                    {
                                        FieldCode = "PHONE",
                                        Values =
                                        [
                                            new ElementValue
                                            {
                                                Value = model.PhoneNumber ?? ""
                                            }
                                        ]

                                    }
                                ]
                            };
                            var createdContact = await _amoCrm.CreateContacts([amoContact]);
                            var clientId = createdContact.Embedded.CreatedContacts.FirstOrDefault().Id;
                            contact.AmoCrmId = clientId.ToString();
                        }
                        await _userManager.AddToRoleAsync(user, "client");
                    }

                    var dblog = new DBLog
                    {
                        Level = "INFO",
                        Message = $"Создан пользователь {user.Email}",
                        User = User.Identity.Name
                    };
                    await _context.DBLog.AddAsync(dblog);
                    await _context.Contacts.AddAsync(contact);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            EditUserViewModel model = new EditUserViewModel { Id = user.Id, Email = user.Email, PhoneNumber = user.PhoneNumber };
            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (contact != null)
            {
                model.IsClient = true;
                model.Name = contact.Name;
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    //user.Year = model.Year;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                        if (contact != null)
                        {
                            var updatedContact = new AmoContact
                            {
                                Id = int.Parse(contact.AmoCrmId),
                                Name = model.Name,
                                CustomFieldsValues = new[]
                                {
                                    new CustomFieldValues
                                    {
                                        FieldCode = "PHONE",
                                        Values = new[]
                                        {
                                            new ElementValue
                                            {
                                                Value = model.PhoneNumber ?? ""
                                            }
                                        }
                                    }
                                }
                            };
                            await _amoCrm.UpdateContacts([updatedContact]);
                            contact.Name = model.Name;
                        }
                        var dblog = new DBLog
                        {
                            Level = "INFO",
                            Message = $"Изменен пользователь {user.Email}",
                            User = User.Identity.Name
                        };
                        await _context.AddAsync(dblog);
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Delete(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {

                var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (contact != null)
                {
                    contact.IsDeleted = true;
                    //var amoContact = await _amoCrm.GetContact(contact.AmoCrmId);

                    //if (amoContact != null)
                    //{
                    //    //amoContact.IsDeleted = true;
                    //    //await _amoCrm.UpdateContacts([amoContact]);
                    //}
                    await _context.SaveChangesAsync();
                }
                var dblog = new DBLog
                {
                    Level = "INFO",
                    Message = $"Удален пользователь {user.Email}",
                    User = User.Identity?.Name
                };
                await _context.DBLog.AddAsync(dblog);
                await _context.SaveChangesAsync();
                IdentityResult result = await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ChangePassword(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ChangePasswordViewModel model = new ChangePasswordViewModel { Id = user.Id, Email = user.Email };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    //IdentityResult result =
                    //    await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    var remPass = await _userManager.RemovePasswordAsync(user);
                    if (!remPass.Succeeded)
                    {
                        foreach (var error in remPass.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    };

                    var addPass = await _userManager.AddPasswordAsync(user, model.NewPassword);
                    if (!addPass.Succeeded)
                    {
                        foreach (var error in addPass.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    };
                    var dblog = new DBLog
                    {
                        Level = "INFO",
                        Message = $"Изменен пароль для пользователя {user.Email}",
                        User = User.Identity?.Name
                    };
                    await _context.DBLog.AddAsync(dblog);
                    await _context.SaveChangesAsync();
                    //if (result.Succeeded)
                    //{
                    //    return RedirectToAction("Index");
                    //}
                    //else
                    //{
                    //    foreach (var error in result.Errors)
                    //    {
                    //        ModelState.AddModelError(string.Empty, error.Description);
                    //    }
                    //}
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Пользователь не найден");
                }
            }
            return View(model);
        }
        async Task GetUserInfo()
        {
            try
            {
                var UserClime = User.FindFirst(ClaimTypes.NameIdentifier);

                // Resolve the user via their email
                if (UserClime != null)
                {
                    _User = await _userManager.FindByIdAsync(UserClime.Value);

                }

            }
            catch (Exception ex)
            {
                await DBConnector.DBLogs.Error($"Не удалось получить информацию по пользователю", ClaimTypes.NameIdentifier, $"{ex.Message}\n{ex.InnerException}");

            }

        }
    }
}
