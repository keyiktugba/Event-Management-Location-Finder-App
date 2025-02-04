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
            var username = User.Identity.Name;
            var user = _context.Users.FirstOrDefault(u => u.UserName == username);

            if (user != null)
            {
                
                var etkinlikBilgileri = _context.Katilimcilar
                    .Where(k => k.KullaniciID == user.Id)
                    .Join(_context.Etkinlikler,
                          k => k.EtkinlikID,
                          e => e.ID,
                          (k, e) => new
                          {
                              e.Konum,
                              e.EtkinlikAdi,
                              e.ID,
                              Katilim = true 
                          })
                    .ToList();

                var katilmadigiEtkinlikler = _context.Etkinlikler
                    .Where(e => !_context.Katilimcilar.Any(k => k.KullaniciID == user.Id && k.EtkinlikID == e.ID))
                    .Select(e => new
                    {
                        e.Konum,
                        e.EtkinlikAdi,
                        e.ID,
                        Katilim = false 
                    })
                    .ToList();

                var etkinlikler = etkinlikBilgileri.Concat(katilmadigiEtkinlikler).ToList();

                var etkinlikKonumlari = etkinlikler.Select(e => e.Konum).ToList();
                var etkinlikAdlari = etkinlikler.Select(e => e.EtkinlikAdi).ToList();
                var etkinlikKatilimDurumu = etkinlikler.Select(e => e.Katilim).ToList();

                if (!string.IsNullOrEmpty(user.Konum))
                {
                    etkinlikKonumlari.Insert(0, user.Konum); 

                }

                ViewData["Konumlar"] = etkinlikKonumlari.Any() ? etkinlikKonumlari : new List<string> { "51.505,-0.09" }; 
                ViewData["EtkinlikAdlari"] = etkinlikAdlari.Any() ? etkinlikAdlari : new List<string> { "Etkinlik Adı Bulunamadı" }; 
                ViewData["KatilimDurumu"] = etkinlikKatilimDurumu;
            }
            else
            {
                ViewData["Konumlar"] = new List<string> { "51.505,-0.09" };
                ViewData["EtkinlikAdlari"] = new List<string> { "Etkinlik Adı Bulunamadı" }; 
                ViewData["KatilimDurumu"] = new List<bool> { false }; 
            }

            return View();
        }

    }
}