using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AVBDelivery.Models;
using Microsoft.AspNetCore.Authorization;
using AVBDelivery.ViewModels;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using Polly;
using AVBDelivery.Jobs;
using AVBDelivery.Models.AmoCrm;

namespace AVBDelivery.Controllers
{
    public class OrganizationsController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<User> _userManager;
        private readonly AmoCrm _amoCrm;
        User _User;
        IList<string> _Roles;

        public OrganizationsController(ApplicationContext context, UserManager<User> userManager, AmoCrm amoCrm)
        {
            _context = context;
            _userManager = userManager;
            _amoCrm = amoCrm;
        }

        //GET: OrganizationsList
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index(string? userId = null)
        {
            ViewBag.UserId = userId;
            var organizations = await GetOrganizationsAsync(userId);
            var model = new OrganizationsViewModel
            {
                Organizations = organizations.ToList(),
                UserId = userId
            };
            return View(model);
        }

        [Authorize(Roles = "admin")]
        private async Task<Organization[]> GetOrganizationsAsync(string? id = null)
        {
            if (id == null)
            {
                var organizations = await _context.Organizations.ToArrayAsync();
                return organizations;
            }
            User? user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var organizations = await _context.Organizations.Where(o => o.Contacts.FirstOrDefault(c => c.UserId == id) != null).ToArrayAsync();
                //var organizations = await _context.Organizations.Where(o => o.Contacts.FirstOrDefault(c => c.UserId == id).).ToArrayAsync();
                return organizations;
            }
            return [];
        }

        //[HttpGet]
        //[Route("/api/getdeliveryminsum")]
        //public async Task<int?> GetDeliveryMinSum(string organizationId)
        //{
        //    var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.OrganizationId == organizationId);
        //    return organization?.MinimalSum;
        //}

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> Create(string? userId = null)
        {
            var allContacts = await _context.Contacts.Where(c => c.IsDeleted == false).ToListAsync();
            var allNotes = await _context.Notes.Where(c => c.IsDeleted == false).ToListAsync();
            var viewModel = new OrganizationEditViewModel()
            {
                AllContacts = allContacts,
                AllNotes = allNotes,
                UserId = userId,
                OrganizationId = Guid.NewGuid().ToString()
                //Organization = new Organization
                //{
                //    OrganizationId = Guid.NewGuid().ToString()
                //}
            };
            return View(viewModel);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create(OrganizationEditViewModel model, List<string>? contacts = null)
        {
            if (model == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var organization = new Organization()
            {
                DeliveryAddress = model.DeliveryAddress,
                DeliveryTime = model.DeliveryTime,
                DeliveryWeekendTime = model.DeliveryWeekendTime,
                Inn = model.Inn,
                Name = model.Name,
                OrganizationId = model.OrganizationId,
                PhoneNumber = model.PhoneNumber,
                Comment = model.Comment,
                MinimalSum = model.MinimalSum,
                Discount = model.Discount,
                Notes = model.Notes
            };
            //var organization = new Organization()
            //{
            //    DeliveryAddress = model.Organization.DeliveryAddress,
            //    DeliveryTime = model.Organization.DeliveryTime,
            //    DeliveryWeekendTime = model.Organization.DeliveryWeekendTime,
            //    Inn = model.Organization.Inn,
            //    Name = model.Organization.Name,
            //    OrganizationId = model.Organization.OrganizationId,
            //    PhoneNumber = model.Organization.PhoneNumber,
            //    Comment = model.Organization.Comment,
            //    MinimalSum = model.Organization.MinimalSum,
            //    Discount = model.Organization.Discount
            //};
            var companies = (await _amoCrm.GetCompaniesAsync())?.Embedded?.Companies;
            var customFields = (await _amoCrm.GetCompaniesCustomFields()).Embedded.CustomFields;
            var innField = customFields.FirstOrDefault(f => f.Name == "ИНН");
            var commentField = customFields.FirstOrDefault(f => f.Name == "Комментарий");
            var deliveryTimeField = customFields.FirstOrDefault(f => f.Id == 979845);
            var weekendDeliveryTimeField = customFields.FirstOrDefault(f => f.Id == 979847);
            //Примечание
            //var noteField = customFields.FirstOrDefault(f => f.Id == 979843);

            //var foundedCompany = companies?.FirstOrDefault(c => c.Name == model.Organization.Name);
            //if (foundedCompany != null)
            //{
            //    organization.AmoCrmId = foundedCompany.Id.ToString();
            //    var companyToUpdate = new Company
            //    {
            //        Id = foundedCompany.Id,
            //        CustomFieldsValues = [
            //            new CustomFieldValues
            //            {
            //                FieldCode = "ADDRESS",
            //                Values = [
            //                    new ElementValue
            //                    {
            //                        Value = model.Organization.DeliveryAddress
            //                    }
            //                ]
            //            },
            //            new CustomFieldValues
            //            {
            //                FieldId = commentField.Id,
            //                Values = [
            //                    new ElementValue
            //                    {
            //                        Value = model.Organization.Comment
            //                    }
            //                ]
            //            },
            //            new CustomFieldValues
            //            {
            //                FieldCode = "PHONE",
            //                Values = [
            //                    new ElementValue
            //                    {
            //                        Value = model.Organization.PhoneNumber
            //                    }
            //                ]
            //            },
            //            new CustomFieldValues
            //            {
            //                FieldId = innField.Id,
            //                Values = [
            //                    new ElementValue
            //                    {
            //                        Value = model.Organization.Inn
            //                    }
            //                ]
            //            }
            //        ]
            //    };
            //    await _amoCrm.UpdateCompanies([companyToUpdate]);
            //}
            //else
            //{

            //var companyToCreate = new Company()
            //{
            //    Name = model.Organization.Name
            //};
            var companyToCreate = new Company()
            {
                Name = model.Name
            };
            var customFieldsValues = new List<CustomFieldValues>();
            
            if (model.DeliveryAddress != null)
            {
                customFieldsValues.Add(
                    new CustomFieldValues
                    {
                        FieldCode = "ADDRESS",
                        Values = [
                            new ElementValue
                            {
                                Value = model.DeliveryAddress
                            }
                        ]
                    }
                );
            }

            if (model.PhoneNumber != null)
            {
                customFieldsValues.Add(
                    new CustomFieldValues
                    {
                        FieldCode = "PHONE",
                        Values = [
                            new ElementValue
                            {
                                Value = model.PhoneNumber
                            }
                        ]
                    }
                );
            }

            if (model.Comment != null)
            {
                customFieldsValues.Add(
                    new CustomFieldValues
                    {
                        FieldId = commentField.Id,
                        Values = [
                            new ElementValue
                            {
                                Value = model.Comment
                            }
                        ]
                    }
                );
            }

            if (model.Inn != null)
            {
                customFieldsValues.Add(
                    new CustomFieldValues
                    {
                        FieldId = innField.Id,
                        Values = [
                            new ElementValue
                            {
                                Value = model.Inn
                            }
                        ]
                    }
                );
            }

            if (model.DeliveryTime != null)
            {
                customFieldsValues.Add(
                    new CustomFieldValues
                    {
                        FieldId = deliveryTimeField.Id,
                        Values = [
                            new ElementValue
                            {
                                Value = model.DeliveryTime
                            }
                        ]
                    }
                );
            }

            if (model.DeliveryWeekendTime != null)
            {
                customFieldsValues.Add(
                    new CustomFieldValues
                    {
                        FieldId = weekendDeliveryTimeField.Id,
                        Values = [
                            new ElementValue
                            {
                                Value = model.DeliveryWeekendTime
                            }
                        ]
                    }
                );
            }

            companyToCreate.CustomFieldsValues = customFieldsValues.ToArray();

            var createdCompany = await _amoCrm.CreateCompanies([companyToCreate]);
            if (createdCompany != null)
            {
                organization.AmoCrmId = createdCompany.Embedded.Companies.FirstOrDefault()?.Id.ToString();
            }
            //}
            if (model.UserId != null)
            {
                var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.UserId == model.UserId);
                if (contact != null)
                {
                    organization.Contacts.Add(contact);
                }
            }
            //var addedContacts = model.AllContacts.Where(c => contacts?.Contains(c.Name) == true);
            //if (addedContacts.Any())
            //{
            //    organization.Contacts.Clear();
            //    organization.Contacts.AddRange(addedContacts);
            //}

            //var addedContacts = 
            var dblog = new DBLog
            {
                User = User.Identity?.Name,
                Message = $"Создана организация \"{organization.Name}\"",
                Level = "INFO"
            };
            await _context.DBLog.AddAsync(dblog);
            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { @userId = model.UserId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string organizationId, string? userId = null)
        {
            if (organizationId == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations.FindAsync(organizationId);
            if (organization == null)
            {
                return NotFound();
            }

            _context.Organizations.Remove(organization);
            var dblog = new DBLog
            {
                Level = "INFO",
                Message = $"Удалена организация \"{organization.Name}\"",
                User = User.Identity.Name
            };
            await _context.SaveChangesAsync();
            if (userId != null)
            {
                return RedirectToAction("Index", new { @userId = userId });
            }
            return RedirectToAction("Index");
        }
        //public async Task<IActionResult> Edit(string? userId = null)
        //{
        //    User? user = await _userManager.FindByIdAsync(userId);
        //    if (user != null)
        //    {
        //        var organizations = await _context.Organizations.Where(o => o.Contacts.FirstOrDefault(c => c.UserId == userId) != null).ToListAsync();
        //    }
                
        //    return RedirectToAction("Index");
        //}

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EditOrganization(string organizationId, string? userId = null)
        {
            if (organizationId == null)
            {
                return NotFound();
            }
            var organization = await _context.Organizations.Include(o => o.Notes).FirstOrDefaultAsync(o => o.OrganizationId == organizationId);
            if (organization == null)
            {
                return NotFound();
            }
            var notes = await _context.Notes.ToListAsync();
            var model = new OrganizationEditViewModel()
            {
                Name = organization.Name,
                Inn = organization.Inn,
                Comment = organization.Comment,
                DeliveryAddress = organization.DeliveryAddress,
                DeliveryTime = organization.DeliveryTime,
                DeliveryWeekendTime = organization.DeliveryWeekendTime,
                Discount = organization.Discount,
                IsDeleted = organization.IsDeleted,
                MinimalSum = organization.MinimalSum,
                OrganizationId = organizationId,
                PhoneNumber = organization.PhoneNumber,
                UserId = userId,
                Notes = organization.Notes.ToList(),
                AllNotes = notes,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveEditedOrganization(OrganizationEditViewModel model)
        {
            if (model == null)
            {
                return NotFound();
            }
            var organization = await _context.Organizations
                .Include(o => o.Notes)   // ← ключевой момент
                .FirstOrDefaultAsync(o => o.OrganizationId == model.OrganizationId);
            organization.Inn = model.Inn;
            organization.Name = model.Name;
            organization.DeliveryAddress = model.DeliveryAddress;
            organization.DeliveryTime = model.DeliveryTime;
            organization.DeliveryWeekendTime = model.DeliveryWeekendTime;
            organization.Comment = model.Comment;
            organization.PhoneNumber = model.PhoneNumber;
            organization.MinimalSum = model.MinimalSum;
            organization.Discount = model.Discount;
            if (model.SelectedNoteIds != null)
            {
                var selectedNotes = await _context.Notes
                    .Where(n => model.SelectedNoteIds.Contains(n.Id))
                    .ToListAsync();

                // Получаем текущие ID примечаний
                var currentNoteIds = organization.Notes.Select(n => n.Id).ToHashSet();
                var newNoteIds = selectedNotes.Select(n => n.Id).ToHashSet();

                // Удаляем те, которых нет в новом списке
                foreach (var noteToRemove in organization.Notes.Where(n => !newNoteIds.Contains(n.Id)).ToList())
                {
                    organization.Notes.Remove(noteToRemove);
                }

                // Добавляем те, которых ещё нет
                foreach (var noteToAdd in selectedNotes.Where(n => !currentNoteIds.Contains(n.Id)))
                {
                    organization.Notes.Add(noteToAdd);
                }
            }
            else
            {
                // Если ничего не выбрано – удаляем все связи
                organization.Notes.Clear();
            }

            var customFields = (await _amoCrm.GetCompaniesCustomFields()).Embedded.CustomFields;
            var innField = customFields.FirstOrDefault(f => f.Name == "ИНН");
            var commentField = customFields.FirstOrDefault(f => f.Name == "Комментарий");
            var deliveryTimeField = customFields.FirstOrDefault(f => f.Id == 979845);
            var weekendDeliveryTimeField = customFields.FirstOrDefault(f => f.Id == 979847);


            var companyToUpdate = new Company
            {
                Id = int.Parse(organization.AmoCrmId),
                Name = model.Name,
                CustomFieldsValues = [
                        new CustomFieldValues
                        {
                            FieldCode = "ADDRESS",
                            Values = [
                                new ElementValue
                                {
                                    Value = model.DeliveryAddress ?? ""
                                }
                            ]
                        },
                        new CustomFieldValues
                        {
                            FieldId = commentField.Id,
                            Values = [
                                new ElementValue
                                {
                                    Value = model.Comment ?? ""
                                }
                            ]
                        },
                        new CustomFieldValues
                        {
                            FieldCode = "PHONE",
                            Values = [
                                new ElementValue
                                {
                                    Value = model.PhoneNumber ?? ""
                                }
                            ]
                        },
                        new CustomFieldValues
                        {
                            FieldId = innField.Id,
                            Values = [
                                new ElementValue
                                {
                                    Value = model.Inn ?? ""
                                }
                            ]
                        },
                        new CustomFieldValues
                        {
                            FieldId = deliveryTimeField.Id,
                            Values = [
                                new ElementValue
                                {
                                    Value = model.DeliveryTime ?? ""
                                }
                            ]
                        },
                        new CustomFieldValues
                        {
                            FieldId = weekendDeliveryTimeField.Id,
                            Values = [
                                new ElementValue
                                {
                                    Value = model.DeliveryWeekendTime ?? ""
                                }
                            ]
                        },
                    ]
            };
            await _amoCrm.UpdateCompanies([companyToUpdate]);

            var dblog = new DBLog
            {
                Level = "INFO",
                Message = $"Организация \"{organization.Name}\" изменена",
                User = User.Identity.Name
            };
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { @userId = model.UserId });
        }
    }
}
