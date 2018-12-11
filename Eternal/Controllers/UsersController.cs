using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Eternal.Utility;
using Eternal.Models;
using Eternal.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace Eternal.Controllers
{
    public class UsersController : Controller
    {
        [Authorize]
        public async Task<IActionResult> Index(int id, bool? passwordChanged = false, bool? deckDeleted = false)
        {
            IEnumerable<Deck> decks = await DbHelper.GetUserDecks(id);

            if (passwordChanged.GetValueOrDefault())
            {
                ViewData["PasswordChangedMessage"] = "Password successfully changed";
            }

            if (deckDeleted.GetValueOrDefault())
            {
                ViewData["DeckDeletedMessage"] = "Your deck has been deleted";
            }

            return View(decks);
        }

        [HttpPost]
        public async Task<IActionResult> Register([Bind("Email, Username, Password")]UserPortal registrant)
        {
            User newUser = new User
            {
                Email = registrant.Email,
                Username = registrant.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(registrant.Password, BCrypt.Net.BCrypt.GenerateSalt()),
                Token = Guid.NewGuid().ToString()
            };

            bool emailExists = await DbHelper.EmailExists(registrant.Email);
            bool usernameExists = await DbHelper.UsernameExists(registrant.Username);

            if (emailExists || usernameExists)
            {
                return RedirectToAction("Login", new { duplicateEmail = emailExists, duplicateUsername = usernameExists });
            }

            newUser = await DbHelper.AddUser(newUser);
            await TransactionEmail.SendRegistrationEmail(newUser);

            return RedirectToAction("Login", new { emailSent = true });
        }

        public async Task<IActionResult> Activate(int userId, string token)
        {
            User user = await DbHelper.GetUser(userId);
            if (user.Token == token)
            {
                await DbHelper.ActivateUser(userId);
                return RedirectToAction("Login", new { activated = true });
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> Login(bool? invalidLogin = false, bool? unverified = false, bool? emailSent = false, bool? activated = false, bool? passwordReset = false, string email = null, bool? duplicateEmail = false, bool? duplicateUsername = false)
        {
            if (unverified.GetValueOrDefault())
            {
                User user = await DbHelper.GetUserByEmail(email);

                ViewData["RegisterEmailInputValue"] = "";

                ViewData["UnverifiedEmail"] = email;

                ViewData["UnverifiedMessage"] = "Please activate your account.";
            }
            if (invalidLogin.GetValueOrDefault())
            {
                ViewData["InvalidLoginMessage"] = "Invalid Email or Password!";
            }
            if (emailSent.GetValueOrDefault())
            {
                ViewData["VerificationEmailMessage"] = "An email verification link has been sent to your inbox.";
            }
            if (activated.GetValueOrDefault())
            {
                ViewData["ActivationMessage"] = "Your account has been successfully activated.";
            }
            if (passwordReset.GetValueOrDefault())
            {
                ViewData["PasswordResetMessage"] = "Your password has been successfully reset.";
            }
            if (duplicateUsername.GetValueOrDefault())
            {
                ViewData["DuplicateUsernameMessage"] = "Username already in use!";
            }
            if (duplicateEmail.GetValueOrDefault())
            {
                ViewData["DuplicateEmailMessage"] = "Email already in use!";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            User storedUser = await DbHelper.GetUserByEmail(user.Email);

            if (BCrypt.Net.BCrypt.Verify(user.Password, storedUser.Password) && storedUser.Active == true)
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim("UserID", storedUser.UserID.ToString()),
                    new Claim("Username", storedUser.Username)
                };

                ClaimsPrincipal principal = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }
            else if (BCrypt.Net.BCrypt.Verify(user.Password, storedUser.Password) && storedUser.Active == false)
            {
                return RedirectToAction("Login", new { unverified = true, email = user.Email });
            }
            else
            {
                return RedirectToAction("Login", new { invalidLogin = true });
            }

            return Redirect("/");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }

        [Authorize]
        public IActionResult ChangePassword(bool invalidPassword = false)
        {
            if (invalidPassword)
            {
                ViewData["InvalidPasswordMessage"] = "Old Password is invalid.";
            }

            PasswordChangeModel changePassword = new PasswordChangeModel();
            return View(changePassword);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword([Bind("OldPassword, NewPassword")]PasswordChangeModel changePassword)
        {
            int userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
            User user = await DbHelper.GetUser(userId);

            if (BCrypt.Net.BCrypt.Verify(changePassword.OldPassword, user.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(changePassword.NewPassword, BCrypt.Net.BCrypt.GenerateSalt());
                await DbHelper.ChangePassword(user);
            }
            else
            {
                return RedirectToAction("ChangePassword", new { invalidPassword = true });
            }

            return RedirectToAction("Index", new { id = userId, passwordChanged = true });
        }

        public IActionResult ForgotPassword(bool? emailSent = false)
        {
            if (emailSent.GetValueOrDefault())
            {
                ViewData["PasswordResetMessage"] = "Password reset link sent to your inbox.";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            User user = await DbHelper.GetUserByEmail(email);
            await TransactionEmail.SendPasswordResetEmail(user);

            return RedirectToAction("ForgotPassword", new { emailSent = true });
        }

        public async Task<IActionResult> ResetPassword(string userId, string token)
        {
            User user = await DbHelper.GetUser(int.Parse(userId));
            if (token == user.Token)
            {
                return View();
            }
            else
            {
                return Forbid();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(int userId, string newPassword)
        {
            User user = await DbHelper.GetUser(userId);
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword, BCrypt.Net.BCrypt.GenerateSalt());
            await DbHelper.ChangePassword(user);
            return RedirectToAction("Login", new { passwordReset = true });
        }

        public async Task ResendRegistrationEmail(string email)
        {
            User user = await DbHelper.GetUserByEmail(email);
            await TransactionEmail.SendRegistrationEmail(user);
        }
    }
}