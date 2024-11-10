using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yazlab_2.Data;
using System.Linq;

namespace Yazlab_2.Controllers
{
    [Authorize]
    public class MapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MapController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var username = User.Identity.Name; // Giriş yapmış kullanıcının kullanıcı adı
            var user = _context.Users.FirstOrDefault(u => u.UserName == username);

            if (user != null && !string.IsNullOrEmpty(user.Konum))
            {
                ViewData["Konum"] = user.Konum; // Kullanıcının konum bilgisini View'a gönderin
            }
            else
            {
                ViewData["Konum"] = "51.505,-0.09"; // Varsayılan konum (örnek)
            }

            return View();
        }
    }
}