using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Claims;
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
        public virtual ICollection<Notification> Notifications { get; set; }
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

            // Kullanıcının etkinliklerini alıyoruz (Onaylanmış etkinlikler)
            var approvedEvents = await _context.Etkinlikler
                .Where(e => e.IsApproved == true)
                .ToListAsync();

            // Kullanıcının ilgi alanlarını alıyoruz
            var interests = await _context.Ilgiler
                .Where(i => i.ID == userId)   // Kullanıcıya ait ilgi alanları
                .Include(i => i.Category)     // İlgili kategoriler de dahil ediliyor
                .ToListAsync();

            var viewModel = new ProfileViewModel
            {
                User = user,
                Events = approvedEvents,
                Interests = interests // İlgi alanlarını view model'e ekliyoruz
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

        /*   [HttpPost]
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

                       // Kullanıcının ilgi alanlarını çekiyoruz
                       var interests = await _context.Ilgiler
                           .Where(i => i.ID == userId)
                           .Include(i => i.Category)
                           .ToListAsync();

                       // Kullanıcı bilgileri ve ilgi alanlarıyla ProfileViewModel oluşturuyoruz
                       var profileViewModel = new ProfileViewModel
                       {
                           User = user,
                          Interests = interests
                       };

                       var result = await _userManager.UpdateAsync(user);
                       if (result.Succeeded)
                       {
                           return View(profileViewModel); // Profil güncelleme başarılıysa profile view'ına yönlendiriyoruz
                       }

                       foreach (var error in result.Errors)
                       {
                           ModelState.AddModelError("", error.Description);
                       }
                   }
               }

               return View(model);  // Profil güncelleme başarısızsa tekrar formu gösteriyoruz
           }*/

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

            // Kullanıcı daha önce katıldıysa eklemeyin
            var alreadyJoined = _context.Katilimcilar.Any(k => k.KullaniciID == userId && k.EtkinlikID == eventId);
            if (alreadyJoined)
            {
                return RedirectToAction("Oneriler");
            }

            // Katılım tablosuna ekliyoruz
            var katilimci = new Katilimci
            {
                KullaniciID = userId,
                EtkinlikID = eventId
            };

            _context.Katilimcilar.Add(katilimci);
            await _context.SaveChangesAsync();

            // Başarılı katılım sonrası önerilere dönüyoruz
            return RedirectToAction("Oneriler");
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
        public async Task<IActionResult> EditUser()
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

            // Kullanıcının ilgi alanlarını alıyoruz
            var userInterests = await _context.Ilgiler
                .Where(i => i.ID == userId)
                .ToListAsync();

            // Kategorileri alıyoruz
            var categories = await _context.Kategoriler.ToListAsync();

            var model = new ProfileViewModel
            {
                User = user,
                SelectedInterests = userInterests.Select(i => i.CategoryID).ToList(),  // Kullanıcının seçtiği ilgi alanlarının ID'leri
                Categories = categories,  // Kategoriler
                ProfilePicture = null // Şu an için null, formda mevcut fotoğrafı göstermeyi unutmayın
            };

            return View(model);
        }



        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> EditUser(ProfileViewModel model)
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

            // Kullanıcı bilgilerini güncelle
            user.UserName = model.User.UserName;
            user.Email = model.User.Email;
            user.FirstName = model.User.FirstName;
            user.LastName = model.User.LastName;

            // Profil fotoğrafı güncellenmesi
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                var folderPath = Path.Combine("wwwroot", "images", "profiles");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, model.ProfilePicture.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                user.ProfilePicture = $"/images/profiles/{model.ProfilePicture.FileName}";
            }

            // İlgi alanlarını güncelle
            var currentInterests = await _context.Ilgiler
                .Where(i => i.ID == userId)
                .ToListAsync();

            // Seçilen ilgi alanlarını alın
            var selectedInterestIds = model.SelectedInterests;

            // Önce mevcut ilgi alanlarını sil
            foreach (var interest in currentInterests)
            {
                _context.Ilgiler.Remove(interest);
            }

            // Yeni seçilen ilgi alanlarını ekle
            foreach (var selectedCategoryId in selectedInterestIds)
            {
                _context.Ilgiler.Add(new Interest
                {
                    ID = userId,
                    CategoryID = selectedCategoryId
                });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi!";
            return RedirectToAction("Profile");
        }
        [HttpGet]
        public IActionResult IlgiAlaninaGoreOneriler()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kullanıcının ilgi alanlarını al
            var userInterests = _context.Ilgiler
                .Where(i => i.ID == userId)
                .Select(i => i.CategoryID)
                .ToList();

            // İlgi alanlarına uygun etkinlikleri al
            var recommendations = _context.Etkinlikler
                .Where(e => userInterests.Contains(e.CategoryID) && e.Tarih >= DateTime.Now) // Gelecek etkinlikler
                .ToList();

            return View(recommendations);
        }
        [HttpGet]
        public IActionResult GecmisEtkinliklereGoreOneriler()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kullanıcının geçmişte katıldığı etkinliklerin kategorilerini al
            var pastEventCategories = _context.Katilimcilar
                .Where(k => k.KullaniciID == userId)
                .Include(k => k.Etkinlik)
                .Select(k => k.Etkinlik.CategoryID)
                .Distinct()
                .ToList();

            // Gelecekteki benzer kategorideki etkinlikleri öner
            var recommendations = _context.Etkinlikler
                .Where(e => pastEventCategories.Contains(e.CategoryID) && e.Tarih >= DateTime.Now)
                .ToList();

            return View(recommendations);
        }

        [HttpGet]
        public IActionResult KonumaGoreOneriler()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kullanıcının konumunu al
            var userLocation = _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Konum)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(userLocation))
            {
                return BadRequest("Kullanıcının konum bilgisi mevcut değil.");
            }

            // Kullanıcının enlem ve boylam bilgilerini ayır
            var userCoordinates = ParseCoordinates(userLocation);

            // Tüm etkinliklerin listesini al ve koordinatlarını kontrol et
            var recommendations = _context.Etkinlikler
                .AsEnumerable() // Veritabanı sorgusundan sonra LINQ kullanmak için
                .Where(e =>
                {
                    var eventCoordinates = ParseCoordinates(e.Konum);

                    // Haversine formülü ile mesafeyi hesapla (30 km'ye kadar olan etkinlikler)
                    var distance = CalculateDistance(userCoordinates.Latitude, userCoordinates.Longitude, eventCoordinates.Latitude, eventCoordinates.Longitude);
                    return distance <= 30 && e.Tarih >= DateTime.Now;
                })
                .ToList();

            return View(recommendations);
        }
        private static (double Latitude, double Longitude) ParseCoordinates(string location)
        {
            var parts = location.Split(',');
            if (parts.Length == 2 &&
                double.TryParse(parts[0], out double latitude) &&
                double.TryParse(parts[1], out double longitude))
            {
                return (latitude, longitude);
            }
            throw new ArgumentException("Geçersiz konum formatı.");
        }
        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Dünya'nın yarıçapı (km cinsinden)
            var lat1Rad = ToRadians(lat1);
            var lon1Rad = ToRadians(lon1);
            var lat2Rad = ToRadians(lat2);
            var lon2Rad = ToRadians(lon2);

            var dLat = lat2Rad - lat1Rad;
            var dLon = lon2Rad - lon1Rad;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // Mesafe km cinsinden
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }


        [HttpGet]
        [Authorize(Roles = "User")]
        public IActionResult Oneriler()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kullanıcının katıldığı etkinliklerin ID'lerini alın
            var joinedEventIds = _context.Katilimcilar
                .Where(k => k.KullaniciID == userId)
                .Select(k => k.EtkinlikID)
                .ToList();

            // İlgi alanlarına göre öneriler
            var userInterests = _context.Ilgiler
                .Where(i => i.ID == userId)
                .Select(i => i.CategoryID)
                .ToList();

            var interestRecommendations = _context.Etkinlikler
                .Where(e => userInterests.Contains(e.CategoryID) && e.Tarih >= DateTime.Now && !joinedEventIds.Contains(e.ID))
                .ToList();

            // Geçmiş etkinliklere göre öneriler
            var pastEventCategories = _context.Katilimcilar
                .Where(k => k.KullaniciID == userId)
                .Include(k => k.Etkinlik)
                .Select(k => k.Etkinlik.CategoryID)
                .Distinct()
                .ToList();

            var pastEventRecommendations = _context.Etkinlikler
                .Where(e => pastEventCategories.Contains(e.CategoryID) && e.Tarih >= DateTime.Now && !joinedEventIds.Contains(e.ID))
                .ToList();

            // Konuma göre öneriler
            var userLocation = _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Konum)
                .FirstOrDefault();

            var locationRecommendations = _context.Etkinlikler
                .Where(e => e.Konum == userLocation && e.Tarih >= DateTime.Now && !joinedEventIds.Contains(e.ID))
                .ToList();

            // ViewModel ile tüm önerileri birleştir
            var model = new RecommendationViewModel
            {
                InterestBased = interestRecommendations,
                PastEventBased = pastEventRecommendations,
                LocationBased = locationRecommendations
            };

            return View(model);
        }



    }
}