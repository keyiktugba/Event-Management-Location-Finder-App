using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yazlab_2.Data;
using Yazlab_2.Models.EntityBase;
using System.Linq;

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
        
        public IActionResult Dashboard()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

      
    }
}
