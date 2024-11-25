using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yazlab_2.Data;
using Yazlab_2.Models;
using System.Linq;
using Yazlab_2.Models.EntityBase;
using Microsoft.EntityFrameworkCore;

namespace Yazlab_2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin Paneline Yönlendirme (Tüm Kullanıcıları Görüntüleme)
        [HttpGet]
        public IActionResult Dashboard()
        {
            var users = _context.Users.ToList(); // Tüm kullanıcıları al
            return View(users); // View'da göster
        }

        // Kullanıcı Silme
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

                    // Kullanıcının katıldığı etkinliklerden kendisini sil
                    // Önce Katilimcilar tablosundaki ilgili kayıtları sil
                    var katilimcilar = _context.Katilimcilar.Where(k => k.KullaniciID== userId).ToList();
                    _context.Katilimcilar.RemoveRange(katilimcilar);


                    // Kullanıcıyı sil
                    _context.Users.Remove(user);

                    // Değişiklikleri kaydet
                    _context.SaveChanges();

                    // Transaction'ı onayla
                    transaction.Commit();
                }
                catch (Exception)
                {
                    // Hata durumunda transaction'ı geri al
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
        public IActionResult ApproveEvent(int id)
        {
            var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.ID == id);
            if (etkinlik != null)
            {
                etkinlik.IsApproved = true;
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Etkinlik başarıyla onaylandı!";
            }
            else
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı!";
            }
            return RedirectToAction("PendingEvents");
        }
        // Tüm Etkinlikleri Görüntüleme (GET)
        [HttpGet]
        public IActionResult Events()
        {
            var events = _context.Etkinlikler
                .Include(e => e.Category)
                .Include(e => e.User)
                .ToList();
            return View(events); // View'e gönder
        }

        // Etkinlik Silme (POST)
        [HttpPost]
        public IActionResult DeleteEvent(int id)
        {
            var etkinlik = _context.Etkinlikler.Find(id);
            if (etkinlik == null)
            {
                return NotFound();
            }
            var mesajlar = _context.Mesajlar.Where(m => m.EtkinlikID == id).ToList();

            // Mesajları sil
            if (mesajlar.Any())
            {
                _context.Mesajlar.RemoveRange(mesajlar);
            }

            // Önce Katilimcilar tablosundaki ilgili kayıtları sil
            var katilimcilar = _context.Katilimcilar.Where(k => k.EtkinlikID == id).ToList();
            _context.Katilimcilar.RemoveRange(katilimcilar);

            // Sonra etkinliği sil
            _context.Etkinlikler.Remove(etkinlik);
            _context.SaveChanges();

            return RedirectToAction("Events");
        }
     
        [HttpGet]
        public IActionResult AddCategory()
        {
            return View(); // Formu döndürür
        }

        // Kategori Ekleme (POST)
        [HttpPost]
        public IActionResult AddCategory(Kategori kategori)
        {
            if (ModelState.IsValid)
            {
                _context.Kategoriler.Add(kategori); // Kategoriyi ekler
                _context.SaveChanges(); // Veritabanına kaydeder
                TempData["SuccessMessage"] = "Kategori başarıyla eklendi!";
                return RedirectToAction("AddCategory"); // Aynı sayfaya yönlendirir
            }
            else
            {
                TempData["ErrorMessage"] = "Kategori eklenirken bir hata oluştu!";
                return View(kategori); // Formu tekrar gösterir
            }
        }


    }
}
