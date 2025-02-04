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
using Microsoft.AspNetCore.Hosting; 

namespace Yazlab_2.Controllers
{
    public class EtkinlikController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        public EtkinlikController(UserManager<User> userManager, ApplicationDbContext context,IWebHostEnvironment env)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
        }
       
        [HttpGet]
        public IActionResult Index()
        {
            var etkinlikler = _context.Etkinlikler
                .Where(e => e.IsApproved == true)
                .Include(e => e.Category)
                .Include(e => e.User)
                .ToList();
            return View(etkinlikler);
        }


        public IActionResult Details(int id)
        {
            var etkinlik = _context.Etkinlikler
                                   .Where(e => e.ID == id)
                                   .FirstOrDefault();

            if (etkinlik == null)
            {
                return NotFound();
            }

        
            var kategori = _context.Kategoriler
                                   .Where(k => k.CategoryID == etkinlik.CategoryID)
                                   .FirstOrDefault();

           
            var konum = etkinlik.Konum.Split(',');
            double latitude = Convert.ToDouble(konum[0].Trim());
            double longitude = Convert.ToDouble(konum[1].Trim());

           
            var etkinlikDetay = new EtkinlikDetayViewModel
            {
                EtkinlikAdi = etkinlik.EtkinlikAdi,
                Aciklama = etkinlik.Aciklama,
                Tarih = etkinlik.Tarih,
                Saat = etkinlik.Saat,
                EtkinlikSuresi = etkinlik.EtkinlikSuresi,
                KategoriAdi = kategori?.CategoryName,
                Konum = etkinlik.Konum, 
                Latitude = latitude,   
                Longitude = longitude,  
                EtkinlikFoto = etkinlik.Foto 
            };

            return View(etkinlikDetay);
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

        
            var model = new EventEditViewModel
            {
                EtkinlikID = etkinlik.ID,
                EtkinlikAdi = etkinlik.EtkinlikAdi,
                Aciklama = etkinlik.Aciklama,
                Tarih = etkinlik.Tarih,
                Saat = etkinlik.Saat,
                EtkinlikSuresi = etkinlik.EtkinlikSuresi,
                Konum = etkinlik.Konum,
                CategoryID = etkinlik.CategoryID,
                UserID = etkinlik.UserID,
                ExistingPicturePath = etkinlik.Foto
            };

            
            ViewData["CategoryList"] = new SelectList(_context.Kategoriler, "CategoryID", "CategoryName", model.CategoryID);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EventEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var etkinlik = _context.Etkinlikler
                    .Where(e => e.ID == model.EtkinlikID)
                    .FirstOrDefault();

                if (etkinlik == null)
                {
                    return NotFound();
                }

            
                etkinlik.EtkinlikAdi = model.EtkinlikAdi;
                etkinlik.Aciklama = model.Aciklama;
                etkinlik.Tarih = model.Tarih;
                etkinlik.Saat = model.Saat;
                etkinlik.EtkinlikSuresi = model.EtkinlikSuresi;
                etkinlik.Konum = model.Konum;
                etkinlik.CategoryID = model.CategoryID;

               
                if (model.EventPicture != null)
                {
                    
                    if (!string.IsNullOrEmpty(etkinlik.Foto) && System.IO.File.Exists(Path.Combine(_env.WebRootPath, etkinlik.Foto.TrimStart('/'))))
                    {
                        System.IO.File.Delete(Path.Combine(_env.WebRootPath, etkinlik.Foto.TrimStart('/')));
                    }

                    var filePath = Path.Combine(_env.WebRootPath, "uploads", model.EventPicture.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.EventPicture.CopyTo(stream);
                    }

                    etkinlik.Foto = "/uploads/" + model.EventPicture.FileName;
                }
                else
                {
                    etkinlik.Foto = model.ExistingPicturePath;
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Etkinliğiniz başarıyla güncellenmiştir.";
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Events", "Admin"); 
                }
                else
                {
                    return RedirectToAction("Profile", "User");
                }
            }

            ViewData["CategoryList"] = _context.Kategoriler
                .Select(k => new SelectListItem
                {
                    Value = k.CategoryID.ToString(),
                    Text = k.CategoryName
                }).ToList();

            return View(model);
        }

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
              
                var model = new EtkinlikRegisterViewModel
                {
                    UserID = User.FindFirstValue(ClaimTypes.NameIdentifier)
                };
                ViewData["CategoryList"] = new SelectList(_context.Kategoriler, "CategoryID", "CategoryName");

                return View(model);
            }
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EtkinlikRegisterViewModel model)
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
                    IsApproved = false
                };

                if (model.EventPicture != null && model.EventPicture.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.EventPicture.FileName);
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.EventPicture.CopyToAsync(stream);
                    }
                    etkinlik.Foto = "/uploads/" + fileName;
                }

                _context.Etkinlikler.Add(etkinlik);
                _context.SaveChanges();

                AddPoints(model.UserID, 15); 

                TempData["SuccessMessage"] = "Etkinliğiniz başarıyla oluşturulmuş ve admin onayına sunulmuştur.";
                return RedirectToAction("Profile", "User");
            }

            ViewData["CategoryList"] = new SelectList(_context.Kategoriler, "CategoryID", "CategoryName");
            return View(model);
        }


        public void AddPoints(string kullaniciId, int points)
        {
            
            var yeniPuan = new Puan
            {
                KullaniciID = kullaniciId,
                Puanlar = points,
                KazanilanTarih = DateTime.Now
            };

            _context.Puanlar.Add(yeniPuan);
            _context.SaveChanges();

            var notificationService = new NotificationService(_context);

            var notificationMessage = $"{points} puan kazandınız! Yeni toplam puanlarınızı kontrol edin.";

            notificationService.AddNotificationAsync(kullaniciId, notificationMessage).Wait();
        }


    }
}


