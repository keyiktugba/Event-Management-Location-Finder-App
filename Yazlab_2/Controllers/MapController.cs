using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yazlab_2.Data;
using System.Linq;
using System.Collections.Generic;

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

            if (user != null)
            {
                // Kullanıcının katıldığı etkinliklerin konumlarını alın
                var etkinlikKonumlari = _context.Katilimcilar
                    .Where(k => k.KullaniciID == user.Id)
                    .Join(_context.Etkinlikler,
                          k => k.EtkinlikID,
                          e => e.ID,
                          (k, e) => e.Konum)
                    .ToList();

                // Kullanıcının kendi konumunu listeye ekleyin
                if (!string.IsNullOrEmpty(user.Konum))
                {
                    etkinlikKonumlari.Insert(0, user.Konum); // Kendi konumunu listenin başına ekle
                }

                ViewData["Konumlar"] = etkinlikKonumlari.Any() ? etkinlikKonumlari : new List<string> { "51.505,-0.09" }; // Varsayılan konum
            }
            else
            {
                ViewData["Konumlar"] = new List<string> { "51.505,-0.09" }; // Varsayılan konum
            }

            return View();
        }

    }
}