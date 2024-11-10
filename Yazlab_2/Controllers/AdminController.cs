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
        [HttpPost]
        public IActionResult DeleteUser(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi!";
            }
            else
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı!";
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
            var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.ID == id);
            if (etkinlik != null)
            {
                _context.Etkinlikler.Remove(etkinlik);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Etkinlik başarıyla silindi!";
            }
            else
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı!";
            }
            return RedirectToAction("Events");
        }


    }
}
