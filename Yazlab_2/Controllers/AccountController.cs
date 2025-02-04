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
        private readonly UserManager<User> _userManager;  
        private readonly SignInManager<User> _signInManager; 
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
                    TempData["WelcomeMessage"] = "Hoşgeldin";
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

        [HttpGet]
        public IActionResult Login()
        {
            return View(new UserLoginViewModel());
        }



        [HttpGet]
        public IActionResult Register()
        {
            var categories = _context.Kategoriler.ToList();
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserRegisterViewModel model, List<int> SelectedCategories)
        {
            if (ModelState.IsValid)
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
                    Konum = model.Konum
                };

                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(stream);
                    }
                    user.ProfilePicture = "/uploads/" + fileName;
                }

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");

                    if (SelectedCategories != null && SelectedCategories.Any())
                    {
                        var interests = SelectedCategories.Select(categoryId => new Interest
                        {
                            ID = user.Id,
                            CategoryID = categoryId
                        }).ToList();

                        _context.Ilgiler.AddRange(interests);
                        await _context.SaveChangesAsync();
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

              
                ViewBag.ErrorMessage = "An error occurred while creating the user. Please try again.";
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
               
                ViewBag.ErrorMessage = "Please correct the errors in the form and try again.";
            }

            ViewBag.Categories = _context.Kategoriler.ToList();
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
                Konum = "41.103823, 29.024271",
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
               
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);  
                    ModelState.AddModelError("", error.Description);  
                }
                return Json(new { success = false, message = "Kayıt Başarısız", errors = result.Errors.Select(e => e.Description).ToList() });
            }

        }

        
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
                
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "E-posta adresi bulunamadı.");
                    return View(model);
                }

                
                string token = await _userManager.GeneratePasswordResetTokenAsync(user);
                string resetLink = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);

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

