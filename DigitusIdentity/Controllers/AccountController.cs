using DigitusIdentity.EmailServices;
using DigitusIdentity.Extensions;
using DigitusIdentity.Identity;
using DigitusIdentity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DigitusIdentity.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AccountController : Controller
    {

        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;
        private IEmailSender _emailsender;
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailsender = emailSender;
        }

        [HttpGet]
        public IActionResult Login(string ReturnUrl=null)
        {
            return View(new LoginModel()
            {
                ReturnUrl = ReturnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "user not found");
                return View(model);
            }
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("", "Please confirm your account");
                return View(model);
            }
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);
            if (result.Succeeded)
            {
                return Redirect(model.ReturnUrl??"~/");
            }
            ModelState.AddModelError("", "username or password is incorrect");
            return View(model);
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,

            };
            var result = await _userManager.CreateAsync(user,model.Password);
            if (result.Succeeded)
            {
                //info about confirmation
                TempData.Put("message", new AlertMessage()
                {
                    Title = "We sent you an E-Mail!",
                    Message = "Please check your mail box",
                    AlertType = "warning"
                });

                //generate token
                var code  = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var url = Url.Action("ConfirmEmail", "Account", new
                {
                    userId = user.Id,
                    token = code
                });
                
                //mail
                await _emailsender.SendEmailAsync(model.Email, "Please confirm your account.", $"Click the <a href='https://localhost:44369{url}'> link </a> to confirm your account");
                return RedirectToAction("Login", "Account");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout(LoginModel model)
        {
            await _signInManager.SignOutAsync();
            TempData.Put("message", new AlertMessage()
            {
                Title = "Logged Out.",
                Message = "You have been succesfully logged out.",
                AlertType = "warning"
            });
            return Redirect("~/");
        }


        public async Task<IActionResult> ConfirmEmail(string userId,string token)
        {
            if (userId==null||token==null)
            {
                TempData.Put("message", new AlertMessage()
                {
                    Title = "Invalid Token.",
                    Message = "Invalid Token.",
                    AlertType = "danger"
                });
                return View();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    TempData.Put("message", new AlertMessage()
                    {
                        Title = "Your account has been confirmed,",
                        Message = "Your account has been confirmed.",
                        AlertType = "success"
                    });
                    return View();
                }
            }
            TempData.Put("message", new AlertMessage()
            {
                Title = "Your account has not been confirmed!",
                Message = "Your account has not been confirmed!",
                AlertType = "warning"
            });
            return View();
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return View();
            }

            var user = await _userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                return View();
            }

            TempData.Put("message", new AlertMessage()
            {
                Title = "We sent you an E-Mail!",
                Message = "Please check your mail box",
                AlertType = "warning"
            });

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var url = Url.Action("ResetPassword", "Account", new
            {
                userId = user.Id,
                token = code
            });
            // email
            await _emailsender.SendEmailAsync(Email, "Reset Password", $"Click the <a href='https://localhost:44369{url}'>link</a> to reset your password");

            return View();
        }
        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPasswordModel { Token = token };

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("Home", "Index");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
            {
                TempData.Put("message", new AlertMessage()
                {
                    Title = "Your password has changed!",
                    Message = "",
                    AlertType = "warning"
                });
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
