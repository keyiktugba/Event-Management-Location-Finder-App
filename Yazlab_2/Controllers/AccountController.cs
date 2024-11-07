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
    

        [HttpGet]
       public async Task<IActionResult> adminRegister()
        {

        
            var user = new User
            {
                UserName ="Admins",
                FirstName = "Admin",
                LastName = "Admin",
                Email ="admin@gmail.com",
                BirthDate = DateTime.Now,
                Gender = "Female",
                PhoneNumber = "05315857939",
                Interests = "spor",
                ProfilePicture = ".",
            };

            IdentityResult result = await _userManager.CreateAsync(user, "AWDj#BBGAq2q2C");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                return Json(new { success = true, message = "Kayıt Başarılı", redirectUrl = Url.Action("AdminProfile", "Admin") });
            }
            else
            {
                // Hataları loglayın veya kullanıcıya gösterin
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);  // Hataları loglar
                    ModelState.AddModelError("", error.Description);  // Hataları ModelState'e ekler
                }
                return Json(new { success = false, message = "Kayıt Başarısız", errors = result.Errors.Select(e => e.Description).ToList() });
            }

        }

        // Çıkış Yapma
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
