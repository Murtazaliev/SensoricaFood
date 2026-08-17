using AVBDelivery.Models;
using AVBDelivery.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AVBDelivery.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationContext _context;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        [HttpGet]
        public IActionResult Register()
        {
            var UserClime = User.FindFirst(ClaimTypes.NameIdentifier);
            if (UserClime != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                var UserClime = User.FindFirst(ClaimTypes.NameIdentifier);
                if (UserClime != null)
                {
                    return RedirectToAction("Index", "Home");
                }
                if (ModelState.IsValid)
                {
                    User user = new User { Email = model.Email, UserName = model.Email };
                    // добавляем пользователя
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        // генерация токена для пользователя
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action(
                            "ConfirmEmail",
                            "Account",
                            new { userId = user.Id, code = code },
                            protocol: HttpContext.Request.Scheme);
                        EmailService emailService = new EmailService();
                        var emailTemplate = await _context.EmailTemplates.FirstOrDefaultAsync(x=>x.Name== "Register");
                        if (emailTemplate != null)
                        {
                            string emailBody = emailTemplate.Template.Replace("${__link}", callbackUrl);
                            await emailService.SendEmailAsync(model.Email, "Подтверждение регистрации", emailBody, user.Id);
                        }
                        else
                        {
                            await emailService.SendEmailAsync(model.Email, "Подтверждение регистрации",
                                $"<div class=\"controls js-body js-body-email\"><div class=\"richtext form-control\"><div contenteditable=\"true\" data-name=\"perform::notification.email::body\"><table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" bgcolor=\"#F6F6F6\"><tbody><tr><td valign=\"top\" align=\"center\" style=\"background-color:rgb(246, 246, 246);\"><table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" bgcolor=\"#FFFFFF\"><tbody><tr><td align=\"left\">&nbsp;</td></tr><tr><td align=\"left\" style=\"padding-top:12px;padding-right:24px;padding-bottom:12px;padding-left:24px;\"><h3>Добро пожаловать в Даша Lite</h3><div>Для завершения регистрации перейдите по ссылке</div><div><strong style=\"background-color: transparent;\"><a href='{callbackUrl}'>Завершить регистрацию</a></strong></div></td></tr></tbody></table></td></tr></tbody></table></div></div></div>",
                                user.Id);
                        }

                        return Content("Для завершения регистрации проверьте электронную почту и перейдите по ссылке, указанной в письме");
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
            catch (System.Exception ex)
            {
                await DBConnector.DBLogs.Error($"Не удалось зарегистрировать пользователя", model.Email,$"{ex.Message}\n{ex.InnerException}") ;

            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            try
            {
                if (userId == null || code == null)
                {
                    return View("Error");
                }
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return View("Error");
                }
                var result = await _userManager.ConfirmEmailAsync(user, code);
                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");
                else
                    return View("Error");
            }
            catch (System.Exception ex)
            {
                await DBConnector.DBLogs.Error($"Не удалось подтвердить Email пользователя", userId, $"{ex.Message}\n{ex.InnerException}");
                return View("Error");
            }

        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            try
            {
                var UserClime = User.FindFirst(ClaimTypes.NameIdentifier);
                if (UserClime != null)
                {
                    return RedirectToAction("Index", "Home");
                }
                return View(new LoginViewModel { ReturnUrl = returnUrl });
            }
            catch (System.Exception ex)
            {
                await DBConnector.DBLogs.Error($"Не удалось авторизовать пользователя",string.Empty, $"{ex.Message}\n{ex.InnerException}");
                return View("Error");
            }

        }
        
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View(NotFound());

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                var UserClime = User.FindFirst(ClaimTypes.NameIdentifier);
                if (UserClime != null)
                {
                    return RedirectToAction("Index", "Home");
                }
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByNameAsync(model.Email);
                    if (user != null)
                    {
                        // проверяем, подтвержден ли email
                        if (!await _userManager.IsEmailConfirmedAsync(user))
                        {
                            ModelState.AddModelError(string.Empty, "Вы не подтвердили свой email");
                            await DBConnector.DBLogs.Info($"Попытка входа без подтверждения Email", model.Email);
                            return View(model);
                        }
                    }

                    var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        // проверяем, принадлежит ли URL приложению
                        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return Redirect(model.ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                    }
                }
                return View(model);
            }
            catch (System.Exception ex)
            {
                await DBConnector.DBLogs.Error($"Не удалось подтвердить Email пользователя", model.Email, $"{ex.Message}\n{ex.InnerException}");
                return View("Error");

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // удаляем аутентификационные куки
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
