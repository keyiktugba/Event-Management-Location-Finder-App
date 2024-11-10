using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using Yazlab_2.Data;
using Yazlab_2.Models.EntityBase;
using Yazlab_2.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

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


        // Profil Görüntüleme İşlemi

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            // Sadece onaylanmış etkinlikleri getiriyoruz
            var approvedEvents = _context.Etkinlikler
                .Where(e => e.IsApproved == true)
                .ToList();

            var viewModel = new ProfileViewModel
            {
                User = user,
                Events = approvedEvents
            };

            return View(viewModel);
        }

        // Kullanıcının kendi oluşturduğu etkinlikleri listeleme (Sadece onaylanmış etkinlikler gösteriliyor)
        [HttpGet]
        [Authorize(Roles = "User")]
        public IActionResult MyEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kullanıcının oluşturduğu ve onaylanmış etkinlikleri filtreliyoruz
            var userEvents = _context.Etkinlikler
                .Where(e => e.UserID == userId && e.IsApproved == true)
                .ToList();

            return View(userEvents);
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
                    // Kullanıcı bilgilerini güncelliyoruz
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.BirthDate = model.BirthDate;
                    user.Gender = model.Gender;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Konum = model.Konum;

                    // Profil fotoğrafı güncelleniyor
                    if (model.ProfilePicture != null)
                    {
                        user.ProfilePicture = model.ProfilePicture;  // Fotoğraf kaydediliyor
                    }

                 

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Dashboard"); // Profil güncellenince dashboard'a yönlendirilir
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model);  // Profil güncelleme başarısızsa tekrar formu gösteriyoruz
        }

        public IActionResult Olustur()
        {
            return RedirectToAction("Create", "Etkinlik");
        }

    
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> JoinEvent(int eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kullanıcı ve etkinlik bilgilerini alıyoruz
            var user = await _userManager.FindByIdAsync(userId);
            var etkinlik = await _context.Etkinlikler.FindAsync(eventId);

            if (user == null || etkinlik == null)
            {
                return NotFound("Kullanıcı veya etkinlik bulunamadı.");
            }

            // Katılım tablosuna ekliyoruz
            var katilimci = new Katilimci
            {
                KullaniciID = userId,
                EtkinlikID = eventId
            };

            _context.Katilimcilar.Add(katilimci);
            await _context.SaveChangesAsync();

            // Katılım işlemi başarılı
            return RedirectToAction("Profile");
        }
        [HttpGet]
        [Authorize(Roles = "User")]
        public IActionResult JoinedEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kullanıcının katıldığı etkinlikleri alıyoruz
            var joinedEvents = _context.Katilimcilar
                .Where(k => k.KullaniciID == userId)
                .Include(k => k.Etkinlik)
                .ThenInclude(e => e.User) // Etkinliği oluşturan kullanıcıyı da dahil ediyoruz
                .Select(k => k.Etkinlik)
                .ToList();

            return View(joinedEvents);
        }
        [HttpGet]
        [Authorize(Roles = "User")]
        public IActionResult Messages(int eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Etkinlikteki katılımcıyı kontrol ediyoruz
            var isParticipant = _context.Katilimcilar
                .Any(k => k.KullaniciID == userId && k.EtkinlikID == eventId);

            if (!isParticipant)
            {
                return RedirectToAction("Profile"); // Eğer kullanıcı etkinliğe katılmadıysa profile yönlendirilsin
            }

            var eventMessages = _context.Mesajlar
                .Where(m => m.EtkinlikID == eventId)
                .Include(m => m.Gonderici)
                .OrderBy(m => m.GonderimZamani)
                .ToList();

            var model = new EventMessagesViewModel
            {
                EventId = eventId,
                Messages = eventMessages
            };

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> SendMessage(int eventId, string message)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Katılımcı olup olmadığını kontrol ediyoruz
            var isParticipant = _context.Katilimcilar
                .Any(k => k.KullaniciID == userId && k.EtkinlikID == eventId);

            if (!isParticipant)
            {
                return RedirectToAction("Profile");
            }

            var newMessage = new Mesaj
            {
                GondericiID = userId,
                EtkinlikID = eventId,
                MesajMetni = message,
                GonderimZamani = DateTime.Now
            };

            _context.Mesajlar.Add(newMessage);
            await _context.SaveChangesAsync();

            return RedirectToAction("Messages", new { eventId });
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public IActionResult EditUser(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user); // Kullanıcı bilgilerini içeren view
        }
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> EditUser(User model, IFormFile ProfilePicture)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Id == model.Id);
            if (existingUser != null)
            {
                // Kullanıcı bilgilerini güncelle
                existingUser.UserName = model.UserName;
                existingUser.Email = model.Email;
                existingUser.PhoneNumber = model.PhoneNumber;
                existingUser.FirstName = model.FirstName;
                existingUser.LastName = model.LastName;
                existingUser.BirthDate = model.BirthDate;
                existingUser.Gender = model.Gender;
                existingUser.Konum = model.Konum;

                // Profil fotoğrafı kaydedilecek klasörü kontrol et ve oluştur
                var folderPath = Path.Combine("wwwroot", "images", "profiles");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Yeni profil fotoğrafını kaydet
                if (ProfilePicture != null && ProfilePicture.Length > 0)
                {
                    var filePath = Path.Combine(folderPath, ProfilePicture.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfilePicture.CopyToAsync(stream);
                    }
                    existingUser.ProfilePicture = $"/images/profiles/{ProfilePicture.FileName}";
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi!";
                return RedirectToAction("Profile");
            }

            return View(model);
        }
    }
}
