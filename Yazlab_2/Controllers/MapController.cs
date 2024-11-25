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
                // Kullanıcının katıldığı etkinliklerin konumlarını ve etkinlik adlarını alın
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
                              Katilim = true // Katıldığı etkinlikler
                          })
                    .ToList();

                // Kullanıcının katılmadığı etkinlikleri al
                var katilmadigiEtkinlikler = _context.Etkinlikler
                    .Where(e => !_context.Katilimcilar.Any(k => k.KullaniciID == user.Id && k.EtkinlikID == e.ID))
                    .Select(e => new
                    {
                        e.Konum,
                        e.EtkinlikAdi,
                        e.ID,
                        Katilim = false // Katılmadığı etkinlikler
                    })
                    .ToList();

                // Hem katıldığı hem katılmadığı etkinlikleri birleştirin
                var etkinlikler = etkinlikBilgileri.Concat(katilmadigiEtkinlikler).ToList();

                // Etkinliklerin konumlarını ve adlarını birleştirin
                var etkinlikKonumlari = etkinlikler.Select(e => e.Konum).ToList();
                var etkinlikAdlari = etkinlikler.Select(e => e.EtkinlikAdi).ToList();
                var etkinlikKatilimDurumu = etkinlikler.Select(e => e.Katilim).ToList();

                // Kullanıcının kendi konumunu listeye ekleyin
                if (!string.IsNullOrEmpty(user.Konum))
                {
                    etkinlikKonumlari.Insert(0, user.Konum); // Kendi konumunu listenin başına ekle
                    etkinlikAdlari.Insert(0, "Kendi Konumunuz");
                }

                // ViewData'ya etkinliklerin konumlarını, adlarını ve katılım durumlarını ekleyin
                ViewData["Konumlar"] = etkinlikKonumlari.Any() ? etkinlikKonumlari : new List<string> { "51.505,-0.09" }; // Varsayılan konum
                ViewData["EtkinlikAdlari"] = etkinlikAdlari.Any() ? etkinlikAdlari : new List<string> { "Etkinlik Adı Bulunamadı" }; // Varsayılan etkinlik adı
                ViewData["KatilimDurumu"] = etkinlikKatilimDurumu;
            }
            else
            {
                ViewData["Konumlar"] = new List<string> { "51.505,-0.09" }; // Varsayılan konum
                ViewData["EtkinlikAdlari"] = new List<string> { "Etkinlik Adı Bulunamadı" }; // Varsayılan etkinlik adı
                ViewData["KatilimDurumu"] = new List<bool> { false }; // Varsayılan katılım durumu
            }

            return View();
        }

    }
}
