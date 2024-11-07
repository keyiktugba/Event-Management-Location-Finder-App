using Microsoft.AspNetCore.Mvc;
using Yazlab_2.Data;
using Yazlab_2.Models.EntityBase;
using Yazlab_2.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Yazlab_2.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;  // UserManager eklenmeli
        private readonly SignInManager<User> _signInManager;  // SignInManager eklenmeli

        public AccountController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.Username);

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                   
                    return RedirectToAction("", "Admin");
                }
                else if (await _userManager.IsInRoleAsync(user, "User"))
                {
                    TempData["SuccessMessage"] = "Hoşgeldin";
                    return RedirectToAction("", "User", new { Username = model.Username });
                }
                
                else
                {
                    TempData["ErrorMessage"] = "Giriş başarısız.";
                    return RedirectToAction("Login");
                }
            }

           
            return RedirectToAction("Login");
        }
        // Giriş Sayfası
        [HttpGet]
        public IActionResult Login()
        {
            return View(new UserLoginViewModel());
        }

        // Giriş İşlemi
        //[HttpPost]
        //public async Task<IActionResult> Login(UserLoginViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _userManager.FindByNameAsync(model.Username);  // Username ile User'ı bul
        //        if (user != null)
        //        {
        //            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
        //            if (result.Succeeded)
        //            {
        //                var claims = new List<Claim>
        //                {
        //                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //                    new Claim(ClaimTypes.Name, user.UserName),
        //                    new Claim(ClaimTypes.Role, user.Role)
        //                };

        //                var identity = new ClaimsIdentity(claims, "login");
        //                var principal = new ClaimsPrincipal(identity);
        //                await HttpContext.SignInAsync(principal);

        //                if (user.Role == "Admin")
        //                {
        //                    return RedirectToAction("Index", "Admin");
        //                }
        //                return RedirectToAction("Dashboard", "User");
        //            }
        //        }
        //        ModelState.AddModelError("", "Kullanıcı adı veya şifre yanlış.");
        //    }
        //    return View(model);
        //}

        // Kayıt Sayfası
        [HttpGet]
        public IActionResult Register()
        {
            return View(new UserRegisterViewModel());
        }

        // Kayıt İşlemi
        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterViewModel model)
        {
          
                var user = new User
                {
                    UserName = model.Username,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    BirthDate = model.BirthDate,
                    Gender = model.Gender,
                    PhoneNumber = model.PhoneNumber,
                    Interests = model.Interests,
                    ProfilePicture = model.ProfilePicture,
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Kullanıcıya "User" rolünü ekleyin
                    await _userManager.AddToRoleAsync(user, "User");

                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            
            return View(model);
        }//buraya kodu atıcam dokunma
        [HttpPost]
        public async Task<IActionResult> adminRegister(UserRegisterViewModel model)
        {

            var user = new User
            {
                UserName =" Admin",
                FirstName = "model.FirstName",
                LastName = "model.LastName",
                Email =" model.Email",
                BirthDate = DateTime.Now,
                Gender = "model.Gender",
                PhoneNumber = "model.PhoneNumber",
                Interests = "model.Interests",
                ProfilePicture = "model.ProfilePicture",
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Kullanıcıya "User" rolünü ekleyin
                await _userManager.AddToRoleAsync(user, "User");

                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
        // Çıkış Yapma
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
//deneme