using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yazlab_2.Data;
using Yazlab_2.Models;
using System.Linq;
using Yazlab_2.Models.EntityBase;
using Microsoft.EntityFrameworkCore;
using Yazlab_2.Models.ViewModel;
using System.Security.Claims;

namespace Yazlab_2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;
        public AdminController(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

      
        [HttpGet]
        public IActionResult Dashboard()
        {
            var users = _context.Users.ToList();
            return View(users); 
        }

        public IActionResult DeleteUser(string userId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Kullanıcıyı bul
                    var user = _context.Users.Find(userId);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    // Kullanıcının katılımcı olduğu etkinlikleri bul
                    var katilimcilar = _context.Katilimcilar.Where(k => k.KullaniciID == userId).ToList();
                    _context.Katilimcilar.RemoveRange(katilimcilar);

                    // Kullanıcıya ait etkinlikleri bul
                    var etkinlikler = _context.Etkinlikler.Where(e => e.UserID == userId).ToList();

                    foreach (var etkinlik in etkinlikler)
                    {
                        // Etkinlik ile ilgili mesajları sil
                        var mesajlar = _context.Mesajlar.Where(m => m.EtkinlikID == etkinlik.ID).ToList();
                        _context.Mesajlar.RemoveRange(mesajlar);

                        // Etkinlik katılımcılarını sil
                        var etkinlikKatilimcilar = _context.Katilimcilar.Where(k => k.EtkinlikID == etkinlik.ID).ToList();
                        _context.Katilimcilar.RemoveRange(etkinlikKatilimcilar);
                    }

                    // Etkinlikleri sil
                    _context.Etkinlikler.RemoveRange(etkinlikler);

                    // Kullanıcıyı sil
                    _context.Users.Remove(user);

                    // Değişiklikleri kaydet ve işlemi onayla
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    // Hata durumunda işlemi geri al
                    transaction.Rollback();
                    return StatusCode(500, "Bir hata oluştu.");
                }
            }

            return RedirectToAction("Dashboard");
        }


       


        public IActionResult PendingEvents()
        {
            var pendingEvents = _context.Etkinlikler
                .Where(e => e.IsApproved == false)
                .Include(e => e.Category)
                .Include(e => e.User)
                .ToList();
            return View(pendingEvents);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveEvent(int id)
        {
            var etkinlik = _context.Etkinlikler
                .Include(e => e.User)
                .FirstOrDefault(e => e.ID == id);

            if (etkinlik != null)
            {
                
                etkinlik.IsApproved = true;
                _context.SaveChanges();

              
                var message1 = "Etkinliğiniz başarıyla onaylandı!";
                await _notificationService.NotifyEventCreatorAsync(etkinlik.ID, message1);

                TempData["SuccessMessage"] = "Etkinlik başarıyla onaylandı ve etkinlik sahibine bildirim gönderildi!";
            }
            else
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı!";
            }

            return RedirectToAction("PendingEvents");
        }
        [HttpPost]
        public async Task<IActionResult> RejectEvent(int id)
        {
            var etkinlik = _context.Etkinlikler
                .Include(e => e.User)
                .FirstOrDefault(e => e.ID == id);

            if (etkinlik != null)
            {
              
                etkinlik.IsApproved = false;
                _context.SaveChanges();

              
                var message = $"{etkinlik.EtkinlikAdi} adlı etkinliğiniz reddedildi!";
                await _notificationService.NotifyEventCreatorAsync(etkinlik.ID, message);

                TempData["SuccessMessage"] = "Etkinlik reddedildi ve etkinlik sahibine bildirim gönderildi!";
            }
            else
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı!";
            }

            return RedirectToAction("PendingEvents");
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult EventMessages(int id)
        {
            
            var eventMessages = _context.Mesajlar
                .Where(m => m.EtkinlikID == id)
                .Include(m => m.Gonderici)
                .OrderBy(m => m.GonderimZamani)
                .ToList();

            var model = new EventMessagesViewModel
            {
                EventId = id,
                Messages = eventMessages
            };

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteMessage(int messageId)
        {
            var message = _context.Mesajlar.Find(messageId);
            if (message != null)
            {
                _context.Mesajlar.Remove(message);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Mesaj başarıyla silindi!";
            }
            else
            {
                TempData["ErrorMessage"] = "Mesaj bulunamadı!";
            }

            return RedirectToAction("EventMessages", new { id = message.EtkinlikID });
        }


        [HttpGet]
        public IActionResult Events()
        {
            var events = _context.Etkinlikler
                .Include(e => e.Category)
                .Include(e => e.User)
                .ToList();
            return View(events);
        }

 
        [HttpPost]
        public IActionResult DeleteEvent(int id)
        {
            var etkinlik = _context.Etkinlikler.Find(id);
            if (etkinlik == null)
            {
                return NotFound();
            }
            var mesajlar = _context.Mesajlar.Where(m => m.EtkinlikID == id).ToList();

        
            if (mesajlar.Any())
            {
                _context.Mesajlar.RemoveRange(mesajlar);
            }

            var katilimcilar = _context.Katilimcilar.Where(k => k.EtkinlikID == id).ToList();
            _context.Katilimcilar.RemoveRange(katilimcilar);

            _context.Etkinlikler.Remove(etkinlik);
            _context.SaveChanges();

            return RedirectToAction("Events");
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            return View(); 
        }

      
        [HttpGet]
        public IActionResult Categories()
        {
            var kategoriler = _context.Kategoriler.ToList();
            return View(new Tuple<IEnumerable<Kategori>, Kategori>(kategoriler, new Kategori()));
        }

        [HttpPost]
        public IActionResult AddCategory(Kategori kategori)
        {
            if (ModelState.IsValid)
            {
                _context.Kategoriler.Add(kategori);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Kategori başarıyla eklendi!";
                return RedirectToAction("Categories");
            }

            TempData["ErrorMessage"] = "Kategori eklenirken bir hata oluştu!";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public IActionResult UpdateCategory(Kategori kategori)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = _context.Kategoriler.Find(kategori.CategoryID);
                if (existingCategory != null)
                {
                    existingCategory.CategoryName = kategori.CategoryName;
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Kategori başarıyla güncellendi!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Güncellenmek istenen kategori bulunamadı!";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Kategori güncellenirken bir hata oluştu!";
            }

            return RedirectToAction("Categories");
        }


        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            var kategori = _context.Kategoriler.Find(id);
            if (kategori == null)
            {
                TempData["ErrorMessage"] = "Kategori bulunamadı!";
                return RedirectToAction("Categories");
            }

            _context.Kategoriler.Remove(kategori);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Kategori başarıyla silindi!";
            return RedirectToAction("Categories");
        }
        [HttpGet]
        public IActionResult Report()
        {
            var today = DateTime.Today;
            var oneWeekAgo = today.AddDays(-7);

            var totalUsers = _context.Users.Count();
            var totalEvents = _context.Etkinlikler.Count();
            var weeklyEvents = _context.Etkinlikler.Count(e => e.CreatedAt >= oneWeekAgo);

            var maleUsers = _context.Users.Count(u => u.Gender == "Male");
            var femaleUsers = _context.Users.Count(u => u.Gender == "Female");

            var ageDistribution = _context.Users
                .GroupBy(u => (DateTime.Now.Year - u.BirthDate.Year) / 10 * 10) 
                .ToDictionary(g => g.Key, g => g.Count());

            var weeklyEventCounts = _context.Etkinlikler
                .Where(e => e.CreatedAt >= oneWeekAgo)
                .GroupBy(e => e.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
                .ToList();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalEvents = totalEvents,
                WeeklyEvents = weeklyEvents,
                MaleUsers = maleUsers,
                FemaleUsers = femaleUsers,
                AgeDistribution = ageDistribution,
                WeeklyEventDates = weeklyEventCounts.Select(w => w.Date).ToList(),
                WeeklyEventCounts = weeklyEventCounts.Select(w => w.Count).ToList()
            };

            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FeedbackNotifications()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _notificationService.GetUserNotificationsAsync(adminId);
            var feedbackNotifications = notifications
                .Where(n => n.Message.Contains("geri bildirim gönderdi"))
                .ToList();

            return View(feedbackNotifications);
        }



    }
}