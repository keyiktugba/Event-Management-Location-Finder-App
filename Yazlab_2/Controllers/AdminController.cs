using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yazlab_2.Data;
using Yazlab_2.Models;
using System.Linq;
using Yazlab_2.Models.EntityBase;

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

        // Kullanıcı Düzenleme
        [HttpGet]
        public IActionResult EditUser(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        public IActionResult EditUser(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser != null)
            {
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                // Diğer alanları da güncellemek için benzer şekilde ekleyebilirsiniz.

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi!";
                return RedirectToAction("Dashboard");
            }
            return View(user);
        }

   
   
    }
}
