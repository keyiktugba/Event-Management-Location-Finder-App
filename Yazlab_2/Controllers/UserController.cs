using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using Yazlab_2.Data;
using Yazlab_2.Models.EntityBase;
using Yazlab_2.Models.ViewModel;

namespace Yazlab_2.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
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
        //        var user = await _userManager.FindByNameAsync(model.Username);
        //        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        //        {
        //            var claims = new List<Claim>
        //            {
        //                new Claim(ClaimTypes.NameIdentifier, user.Id),
        //                new Claim(ClaimTypes.Name, user.UserName),
        //            };

        //            var identity = new ClaimsIdentity(claims, "login");
        //            var principal = new ClaimsPrincipal(identity);
        //            await _signInManager.SignInAsync(user, false); // Kullanıcıyı giriş yaptır

        //            if (user.Role == "Admin")
        //            {
        //                return RedirectToAction("Dashboard", "Admin");
        //            }

        //            return RedirectToAction("Dashboard");
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
      


        // Kullanıcı Profili Görüntüleme
        [Authorize(Roles = "User")]
        public IActionResult Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                return View("Profile", user);
            }
            return NotFound("Kullanıcı bulunamadı.");
        }

        // Profil Güncelleme İşlemi
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Profile(User model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.BirthDate = model.BirthDate;
                    user.Gender = model.Gender;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Interests = model.Interests;
                    user.ProfilePicture = model.ProfilePicture;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Dashboard");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        // Çıkış Yapma İşlemi
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); // Çıkış işlemi
            return RedirectToAction("Login");
        }
    }
}
