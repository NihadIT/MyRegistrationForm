using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyRegistrationForm.Models;
using MyRegistrationForm.ViewModels;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyRegistrationForm.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            //формат для номера телефона
            string numFormat = null;
            string pattern = @"((\+38|8|\+3|\+ )[ ]?)?([(]?\d{3}[)]?[\- ]?)?(\d[ -]?){6,14}";
            if (model.PhoneNumber.Length > 11)
                numFormat = model.PhoneNumber.Insert(6, "-").Insert(3, "-");
            else ModelState.AddModelError(string.Empty, "Неверный формат номера телефона");
            bool phoneFlag = false;
            
            //Добавление пользователя
            if (ModelState.IsValid && Regex.IsMatch(model.PhoneNumber, pattern, RegexOptions.IgnoreCase))
            {
                phoneFlag = true;
                User user = new User { Email = model.Email, PhoneNumber = numFormat, UserName = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            else
            {
                if (phoneFlag == false)
                    ModelState.AddModelError(string.Empty, "Номер телефона должен быть формата: +380*********");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
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
