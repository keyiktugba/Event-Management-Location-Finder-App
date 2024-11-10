using Microsoft.AspNetCore.Mvc;
using Yazlab_2.Data;
using Yazlab_2.Models.EntityBase;
using Yazlab_2.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Yazlab_2.Models.Service;

namespace Yazlab_2.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;  // UserManager eklenmeli
        private readonly SignInManager<User> _signInManager;  // SignInManager eklenmeli
        private readonly EmailService _emailService;
        public AccountController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, EmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
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

                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (await _userManager.IsInRoleAsync(user, "User"))
                {
                    TempData["SuccessMessage"] = "Hoşgeldin";
                    return RedirectToAction("Profile", "User", new { userId = user.Id });
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

        [HttpGet]
        public IActionResult Register()
        {
            // Kategorileri model olarak alıp register view'a gönderebiliriz
            var categories = _context.Kategoriler.ToList();
            ViewBag.Categories = categories;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Yeni kullanıcı oluşturuluyor
                var user = new User
                {
                    UserName = model.Username,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    BirthDate = model.BirthDate,
                    Gender = model.Gender,
                    PhoneNumber = model.PhoneNumber,
                    Konum = model.Konum
                };

                // Profil fotoğrafı yüklenmişse, dosyayı kaydediyoruz
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    // Dosya adı için benzersiz bir GUID oluşturuluyor
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);

                    // Yüklemek için hedef dizini belirliyoruz
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                    // Eğer klasör yoksa oluşturuyoruz
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Dosyanın tam yolunu belirliyoruz
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Dosyayı hedef dizine kaydediyoruz
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(stream);
                    }

                    // Dosya yolunu veritabanına kaydedilmek üzere modelde saklıyoruz
                    user.ProfilePicture = "/uploads/" + fileName;
                }

                // Kullanıcıyı veritabanına kaydediyoruz
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // İlgi alanları, roller ve kullanıcı girişi işlemleri
                    await _userManager.AddToRoleAsync(user, "User");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                // Hata varsa, ModelState'e ekliyoruz
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Kategorileri alıp view'a gönderiyoruz
            var categories = _context.Kategoriler.ToList();
            ViewBag.Categories = categories;

            return View(model);
        }


       

        [HttpGet]
        public async Task<IActionResult> adminRegister()
        {


            var user = new User
            {
                UserName = "Admins",
                FirstName = "Admin",
                LastName = "Admin",
                Email = "admin@gmail.com",
                BirthDate = DateTime.Now,
                Gender = "Female",
                PhoneNumber = "05315857939",
               // ProfilePicture = ".",
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


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

    
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Reset link oluşturulması
                string resetLink = "https://yourapp.com/resetpassword?token=12345"; // Burada linki dinamik yapmalısınız.

                // Şifre sıfırlama e-posta gönderme
                await _emailService.SendResetPasswordEmail(model.Email, resetLink);

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz işlem.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["Message"] = "Şifreniz başarıyla sıfırlandı.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }

}

// Kayıt İşlemi
/*  [HttpPost]
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
*/