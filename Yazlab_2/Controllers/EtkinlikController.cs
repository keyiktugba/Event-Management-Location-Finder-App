using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yazlab_2.Data;
using Yazlab_2.Models.EntityBase;
using Yazlab_2.Models.ViewModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

namespace Yazlab_2.Controllers
{
    public class EtkinlikController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public EtkinlikController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        // 1. Etkinlik Listeleme
        public IActionResult Index()
        {
            var etkinlikler = _context.Etkinlikler
                .Include(e => e.Category)
                .Include(e => e.User)
                .ToList();
            return View(etkinlikler);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var etkinlik = _context.Etkinlikler
                .Where(e => e.ID == id)
                .FirstOrDefault();

            if (etkinlik == null)
            {
                return NotFound();
            }

            // Etkinlik modelini ViewModel'e dönüştür
            var model = new EtkinlikRegisterViewModel
            {   EtkinlikID= etkinlik.ID,
                EtkinlikAdi = etkinlik.EtkinlikAdi,
                Aciklama = etkinlik.Aciklama,
                Tarih = etkinlik.Tarih,
                Saat = etkinlik.Saat,
                EtkinlikSuresi = etkinlik.EtkinlikSuresi,
                Konum = etkinlik.Konum,
                CategoryID = etkinlik.CategoryID,
                UserID = etkinlik.UserID
            };

            // ViewData'yı güncelle
            ViewData["CategoryList"] = new SelectList(_context.Kategoriler, "CategoryID", "CategoryName", model.CategoryID);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EtkinlikRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var etkinlik = _context.Etkinlikler
                    .Where(e => e.ID == model.EtkinlikID)  // ID'yi burada doğru alacağız, kategori değil
                    .FirstOrDefault();

                if (etkinlik == null)
                {
                    return NotFound();
                }

                // Etkinlik modelini güncelle
                etkinlik.EtkinlikAdi = model.EtkinlikAdi;
                etkinlik.Aciklama = model.Aciklama;
                etkinlik.Tarih = model.Tarih;
                etkinlik.Saat = model.Saat;
                etkinlik.EtkinlikSuresi = model.EtkinlikSuresi;
                etkinlik.Konum = model.Konum;
                etkinlik.CategoryID = model.CategoryID;

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Etkinliğiniz başarıyla güncellenmiştir.";
                return RedirectToAction("Profile", "User");
            }

            // Model geçerli değilse kategorileri tekrar yükle
            ViewData["CategoryList"] = _context.Kategoriler
                .Select(k => new SelectListItem
                {
                    Value = k.CategoryID.ToString(),
                    Text = k.CategoryName
                }).ToList();

            return View(model);
        }

        // 4. Etkinlik Silme
        public IActionResult Delete(int id)
        {
            var etkinlik = _context.Etkinlikler
                .Include(e => e.Category)
                .Include(e => e.User)
                .FirstOrDefault(e => e.ID == id);
            if (etkinlik == null)
            {
                return NotFound();
            }
            return View(etkinlik);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik != null)
            {
                _context.Etkinlikler.Remove(etkinlik);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
          public IActionResult Create()
            {
                // Kullanıcının ID'sini alıyoruz
                var model = new EtkinlikRegisterViewModel
                {
                    UserID = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };

                // Kategorileri veritabanından alıp SelectList olarak ViewData'ya ekliyoruz
                ViewData["CategoryList"] = new SelectList(_context.Kategoriler, "CategoryID", "CategoryName");

                return View(model);
            }
        /*
            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult Create(EtkinlikRegisterViewModel model)
            {
                if (ModelState.IsValid)
                {
                    var etkinlik = new Etkinlik
                    {
                        EtkinlikAdi = model.EtkinlikAdi,
                        Aciklama = model.Aciklama,
                        Tarih = model.Tarih,
                        Saat = model.Saat,
                        EtkinlikSuresi = model.EtkinlikSuresi,
                        Konum = model.Konum,
                        CategoryID = model.CategoryID,
                        UserID = model.UserID,
                        CreatedAt = model.CreatedAt
                    };

                    _context.Etkinlikler.Add(etkinlik);
                    _context.SaveChanges();

                    return RedirectToAction("Profile", "User");
                }

                // Model doğrulama başarısızsa, kategorileri tekrar yükle
                ViewData["CategoryList"] = _context.Kategoriler
                    .Select(k => new SelectListItem
                    {
                        Value = k.CategoryID.ToString(),
                        Text = k.CategoryName
                    }).ToList();

                return View(model);
            }*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EtkinlikRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var etkinlik = new Etkinlik
                {
                    EtkinlikAdi = model.EtkinlikAdi,
                    Aciklama = model.Aciklama,
                    Tarih = model.Tarih,
                    Saat = model.Saat,
                    EtkinlikSuresi = model.EtkinlikSuresi,
                    Konum = model.Konum,
                    CategoryID = model.CategoryID,
                    UserID = model.UserID,
                    CreatedAt = DateTime.Now,
                    IsApproved = false // Başlangıçta onaysız olarak ayarlanır
                };

                _context.Etkinlikler.Add(etkinlik);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Etkinliğiniz başarıyla oluşturulmuş ve admin onayına sunulmuştur.";
                return RedirectToAction("Profile", "User");
            }

            ViewData["CategoryList"] = _context.Kategoriler
                .Select(k => new SelectListItem
                {
                    Value = k.CategoryID.ToString(),
                    Text = k.CategoryName
                }).ToList();

            return View(model);
        }


    }
}


