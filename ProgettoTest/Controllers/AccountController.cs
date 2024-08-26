using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProgettoTest.Data;
using ProgettoTest.Models;
using ProgettoTest.ViewModels;

namespace ProgettoTest.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        #region Login
        public IActionResult Login()
        {
            var response = new LoginViewModel();
            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if(!ModelState.IsValid) return View(loginVM);

            var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);

            if(user != null)
            {
                // User is found check password

                var passwordCheck = await _userManager.CheckPasswordAsync(user, loginVM.Password);

                if(passwordCheck)
                {
                    // Password correct, sign in

                    var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);

                    if(result.Succeeded)
                    {
                        return RedirectToAction("Index", "Race");
                    }

                    // Password is incorrect

                    TempData["Error"] = "Wrong credentials. Please, try again";

                    return View(loginVM);
                }
            }

            // User not found

            TempData["Error"] = "Wrong credentials. Please try again";

            return View(loginVM);
        }
        #endregion

        #region Register
        public IActionResult Register()
        {
            var response = new RegisterViewModel();
            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if(!ModelState.IsValid) return View(registerViewModel);

            var user = await _userManager.FindByEmailAsync(registerViewModel.EmailAddress);

            if (user != null)
            {
                TempData["Error"] = "This email is already used";

                return View(registerViewModel);
            }

            var newUser = new AppUser()
            {
                Email = registerViewModel.EmailAddress,
                UserName = registerViewModel.EmailAddress
            };

            var newUserResponse = await _userManager.CreateAsync(newUser, registerViewModel.Password);

            if (newUserResponse.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, UserRoles.User);
            }

            return RedirectToAction("Index", "Race");
        }
        #endregion

        #region Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Race");
        }
        #endregion
    }
}
