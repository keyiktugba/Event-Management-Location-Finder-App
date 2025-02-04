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
using System.Globalization;
using Newtonsoft.Json;

namespace Yazlab_2.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;
        public virtual ICollection<Notification> Notifications { get; set; }

        private readonly NotificationService _notificationService;
        public UserController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _notificationService = notificationService;
        }
        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ProfileDetails()
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

            var interests = await _context.Ilgiler
                .Where(i => i.ID == userId)
                .Include(i => i.Category)
                .ToListAsync();

            var viewModel = new ProfileViewModel
            {
                User = user,
                Interests = interests
            };

            return View(viewModel);
        }

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

            var approvedEvents = await _context.Etkinlikler
                .Where(e => e.IsApproved == true) 
                .ToListAsync();

            var totalPoints = await _context.Puanlar
                .Where(p => p.KullaniciID == userId)
                .SumAsync(p => p.Puanlar);


            var interests = await _context.Ilgiler
                .Where(i => i.ID == userId)
                .Include(i => i.Category)
                .ToListAsync();

         
            var joinedEventIds = await _context.Katilimcilar
                .Where(k => k.KullaniciID == userId)
                .Select(k => k.EtkinlikID)
                .ToListAsync();

  
            var userInterests = interests.Select(i => i.CategoryID).ToList();

            var interestRecommendations = await _context.Etkinlikler
                .Where(e => userInterests.Contains(e.CategoryID) &&
                            e.Tarih >= DateTime.Now &&
                            e.IsApproved == true &&  
                            !joinedEventIds.Contains(e.ID))
                .ToListAsync();

       
            var pastEventCategories = await _context.Katilimcilar
                .Where(k => k.KullaniciID == userId)
                .Include(k => k.Etkinlik)
                .Select(k => k.Etkinlik.CategoryID)
                .Distinct()
                .ToListAsync();

            var pastEventRecommendations = await _context.Etkinlikler
                .Where(e => pastEventCategories.Contains(e.CategoryID) &&
                            e.Tarih >= DateTime.Now &&
                            e.IsApproved == true &&  
                            !joinedEventIds.Contains(e.ID))
                .ToListAsync();

     
            var userLocation = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Konum)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(userLocation))
            {
                return BadRequest("Kullanıcının konum bilgisi mevcut değil.");
            }

            var userCoordinates = ParseCoordinates(userLocation);

            var etkinlikler = await _context.Etkinlikler
                .Where(e => e.Tarih >= DateTime.Now &&
                            e.IsApproved == true &&  
                            !joinedEventIds.Contains(e.ID))
                .ToListAsync();

           
            var locationRecommendations = etkinlikler
                .Where(e =>
                {
                    var eventCoordinates = ParseCoordinates(e.Konum);
                    var distance = CalculateDistance(
                        userCoordinates.Latitude,
                        userCoordinates.Longitude,
                        eventCoordinates.Latitude,
                        eventCoordinates.Longitude
                    );
                    return distance <= 20; 
                })
                .ToList();

           
            var model = new ProfileViewModel
            {
                User = user,
                Events = approvedEvents,  
                Interests = interests,
                TotalPoints = totalPoints,
                Recommendations = new RecommendationViewModel
                {
                    InterestBased = interestRecommendations,
                    PastEventBased = pastEventRecommendations,
                    LocationBased = locationRecommendations
                }
            };

            return View(model);
        }

        public IActionResult Harita(int eventId)
        {
            var etkinlik = _context.Etkinlikler
                                   .FirstOrDefault(e => e.ID == eventId);

            if (etkinlik == null)
            {
                return NotFound();
            }

        
            var etkinlikKonum = etkinlik.Konum;  
            var currentUserId = _userManager.GetUserId(User);

            var kullaniciKonum = (from katilimci in _context.Katilimcilar
                                  join user in _context.Users
                                  on katilimci.KullaniciID equals user.Id
                                  where katilimci.EtkinlikID == eventId && katilimci.KullaniciID == currentUserId
                                  select user.Konum).FirstOrDefault();

            if (string.IsNullOrEmpty(kullaniciKonum))
            {
                kullaniciKonum = "0,0"; 
            }

            var haritaViewModel = new HaritaViewModel
            {
                EtkinlikAdi = etkinlik.EtkinlikAdi,
                Aciklama = etkinlik.Aciklama,
                EtkinlikKonum = etkinlikKonum,
                KullaniciKonum = kullaniciKonum,
                Foto = etkinlik.Foto,
                Tarih = etkinlik.Tarih, 
                Saat = etkinlik.Saat, 
                EtkinlikSuresi = etkinlik.EtkinlikSuresi 
            };


            return View(haritaViewModel);
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public IActionResult MyEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userEvents = _context.Etkinlikler
                .Where(e => e.UserID == userId && e.IsApproved == true)
                .ToList();

            return View(userEvents);
        }
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> LeaveEvent(int eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var participant = await _context.Katilimcilar
                .FirstOrDefaultAsync(k => k.KullaniciID == userId && k.EtkinlikID == eventId);

            if (participant == null)
            {
                ViewBag.ErrorMessage = "Etkinliğe katılmadınız!";
                return RedirectToAction("Profile");
            }


            _context.Katilimcilar.Remove(participant);
            await _context.SaveChangesAsync();

            var pointsToDeduct = 10;  
            DeductPoints(userId, pointsToDeduct);

            ViewBag.SuccessMessage = "Etkinlikten başarıyla çıkıldı!";
            return RedirectToAction("Profile"); 
        }

        private void DeductPoints(string userId, int points)
        {
          
            var userPoints = _context.Puanlar
                .FirstOrDefault(p => p.KullaniciID == userId);

            if (userPoints != null)
            {
                userPoints.Puanlar -= points;
                _context.SaveChanges();
            }
        }

        public IActionResult Olustur()
        {
            return RedirectToAction("Create", "Etkinlik");
        }
        public void AddPoints(string kullaniciId, int points)
        {
          
            var yeniPuan = new Puan
            {
                KullaniciID = kullaniciId,
                Puanlar = points,
                KazanilanTarih = DateTime.Now
            };

    
            _context.Puanlar.Add(yeniPuan);
            _context.SaveChanges();
            var notificationService = new NotificationService(_context);


            var notificationMessage = $"{points} puan kazandınız! Yeni toplam puanlarınızı kontrol edin.";

            
            notificationService.AddNotificationAsync(kullaniciId, notificationMessage).Wait();
        }
       
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> JoinEvent(int eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var etkinlik = await _context.Etkinlikler.FindAsync(eventId);

            if (user == null || etkinlik == null)
            {
                TempData["EventJoinErrorMessage"] = "Kullanıcı veya etkinlik bulunamadı.";
                return RedirectToAction("Profile");
            }

        
            if (etkinlik.Tarih.Add(etkinlik.Saat) < DateTime.Now)
            {
                TempData["EventJoinErrorMessage"] = "Malesef katılamazsınız, etkinlik geçmişte.";
                return RedirectToAction("Profile");
            }

 
            var userEvents = _context.Katilimcilar
                .Where(k => k.KullaniciID == userId)
                .Include(k => k.Etkinlik)
                .Select(k => k.Etkinlik)
                .ToList();

     
            bool isTimeConflict = userEvents.Any(e =>
                (etkinlik.Tarih.Add(etkinlik.Saat) < e.Tarih.Add(e.Saat.Add(TimeSpan.FromMinutes(e.EtkinlikSuresi))) &&
                 etkinlik.Tarih.Add(etkinlik.Saat.Add(TimeSpan.FromMinutes(etkinlik.EtkinlikSuresi))) > e.Tarih.Add(e.Saat)));

            var alreadyJoined = _context.Katilimcilar.Any(k => k.KullaniciID == userId && k.EtkinlikID == eventId);
            if (alreadyJoined)
            {
                TempData["EventJoinErrorMessage"] = "Bu etkinliğe zaten katıldınız!";
                return RedirectToAction("Profile");
            }

            if (isTimeConflict)
            {
                
                TempData["EventJoinErrorMessage"] = "Seçtiğiniz etkinlik, mevcut etkinliklerinizle zaman çakışıyor! Ancak aynı kategorideki başka bir etkinlik öneriyoruz.";

          
                var categoryId = etkinlik.CategoryID;

                
                var joinedEventIds = await _context.Katilimcilar
                    .Where(k => k.KullaniciID == userId)
                    .Select(k => k.EtkinlikID)
                    .ToListAsync();

                var eventRecommendations = await _context.Etkinlikler
                    .Where(e => e.CategoryID == categoryId &&
                                e.Tarih >= DateTime.Now &&  
                                e.IsApproved == true &&     
                                !joinedEventIds.Contains(e.ID)) 
                    .ToListAsync();

                var eventRecommendationsJson = JsonConvert.SerializeObject(eventRecommendations);
                TempData["EventRecommendations"] = eventRecommendationsJson;

                return RedirectToAction("Profile");
            }

    
            var katilimci = new Katilimci
            {
                KullaniciID = userId,
                EtkinlikID = eventId
            };

            _context.Katilimcilar.Add(katilimci);
            await _context.SaveChangesAsync();

     
            AddPoints(userId, _context.Katilimcilar.Count(k => k.KullaniciID == userId) == 1 ? 20 : 10);

            TempData["EventJoinSuccessMessage"] = "Etkinliğe başarıyla katıldınız!";
            return RedirectToAction("Profile");
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> FeedbackForm()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

  
            var pastEvents = await _context.Katilimcilar
                .Where(k => k.KullaniciID == userId)
                .Select(k => k.Etkinlik)
                .Where(e => e.Tarih < DateTime.Now.Date ||
                            (e.Tarih == DateTime.Now.Date && e.Saat < DateTime.Now.TimeOfDay)) 
                .ToListAsync();

            return View(pastEvents);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendFeedback(int eventId, string feedback, int rating)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

           
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("FeedbackForm");
            }

   
            var etkinlik = await _context.Etkinlikler.FindAsync(eventId);
            if (etkinlik == null || etkinlik.Tarih.Add(etkinlik.Saat) > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Geçersiz etkinlik.";
                return RedirectToAction("FeedbackForm");
            }

          
            var isUserJoined = await _context.Katilimcilar
                .AnyAsync(k => k.KullaniciID == userId && k.EtkinlikID == eventId);

            if (!isUserJoined)
            {
                TempData["ErrorMessage"] = "Bu etkinliğe katılmadınız, geri bildirim gönderemezsiniz.";
                return RedirectToAction("FeedbackForm");
            }

 
            var message = $"{user.UserName} adlı kullanıcı '{etkinlik.EtkinlikAdi}' etkinliği için geri bildirim gönderdi: {feedback} (Puan: {rating}/5)";

            
            await _notificationService.AddAdminNotificationAsync(message);

            var pastEvents = await _context.Katilimcilar
                .Where(k => k.KullaniciID == userId)
                .Select(k => k.Etkinlik)
                .Where(e => e.ID != eventId && (e.Tarih < DateTime.Now.Date ||
                                                (e.Tarih == DateTime.Now.Date && e.Saat < DateTime.Now.TimeOfDay))) 
                .ToListAsync();

            TempData["SuccessMessage1"] = "Geri bildiriminiz başarıyla gönderildi!";
            return View("FeedbackForm", pastEvents);  
        }



        [HttpGet]
        [Authorize(Roles = "User")]
        public IActionResult JoinedEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

  
            var joinedEvents = _context.Katilimcilar
                .Where(k => k.KullaniciID == userId)
                .Include(k => k.Etkinlik)
                .ThenInclude(e => e.User) 
                .Select(k => k.Etkinlik)
                .ToList();

            return View(joinedEvents);
        }
        [HttpGet]
        [Authorize(Roles = "User")]
        public IActionResult Messages(int eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var isParticipant = _context.Katilimcilar
                .Any(k => k.KullaniciID == userId && k.EtkinlikID == eventId);

            if (!isParticipant)
            {
                return RedirectToAction("Profile"); 
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
            await _notificationService.NotifyParticipantsAsync(eventId, userId, message);

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

            var userInterests = await _context.Ilgiler
                .Where(i => i.ID == userId)
                .ToListAsync();

            var categories = await _context.Kategoriler.ToListAsync();

            var model = new ProfileViewModel
            {
                User = user,
                SelectedInterests = userInterests.Select(i => i.CategoryID).ToList(),
                Categories = categories,  
                ProfilePicture = null 
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

         
            user.UserName = model.User.UserName;
            user.Email = model.User.Email;
            user.FirstName = model.User.FirstName;
            user.LastName = model.User.LastName;
            user.Konum = model.Konum;
            user.BirthDate=model.User.BirthDate;
            user.PhoneNumber = model.User.PhoneNumber;


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

            var currentInterests = await _context.Ilgiler
                .Where(i => i.ID == userId)
                .ToListAsync();

            var selectedInterestIds = model.SelectedInterests;

            foreach (var interest in currentInterests)
            {
                _context.Ilgiler.Remove(interest);
            }

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

            var userInterests = _context.Ilgiler
                .Where(i => i.ID == userId)
                .Select(i => i.CategoryID)
                .ToList();

            var recommendations = _context.Etkinlikler
                .Where(e => userInterests.Contains(e.CategoryID) && e.Tarih >= DateTime.Now) 
                .ToList();

            return View(recommendations);
        }
        [HttpGet]
        public IActionResult GecmisEtkinliklereGoreOneriler()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pastEventCategories = _context.Katilimcilar
                .Where(k => k.KullaniciID == userId)
                .Include(k => k.Etkinlik)
                .Select(k => k.Etkinlik.CategoryID)
                .Distinct()
                .ToList();

            var recommendations = _context.Etkinlikler
                .Where(e => pastEventCategories.Contains(e.CategoryID) && e.Tarih >= DateTime.Now)
                .ToList();

            return View(recommendations);
        }


private static (double Latitude, double Longitude) ParseCoordinates(string location)
    {
 
        var parts = location.Split(',');

        if (parts.Length == 2)
        {

            string latitudePart = parts[0].Trim();
            string longitudePart = parts[1].Trim();

   
            if (double.TryParse(latitudePart, NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude) &&
                double.TryParse(longitudePart, NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude))
            {
                return (latitude, longitude);
            }
            else
            {
                throw new ArgumentException("Geçersiz enlem ve boylam formatı.");
            }
        }
        else
        {
            throw new ArgumentException("Konum bilgisi geçerli formatta değil.");
        }
    }


    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; 
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; 
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }


    }
}