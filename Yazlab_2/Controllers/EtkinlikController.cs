using Microsoft.AspNetCore.Mvc;
using Yazlab_2.Data;
using Yazlab_2.Models.EntityBase;
using System.Linq;
using System.Threading.Tasks;

namespace Yazlab_2.Controllers
{
    public class EtkinlikController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EtkinlikController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Etkinlik Listeleme
        public IActionResult Index()
        {
            var etkinlikler = _context.Etkinlikler.ToList();
            return View(etkinlikler);
        }

        // 2. Etkinlik Oluşturma (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 2. Etkinlik Oluşturma (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Etkinlik etkinlik)
        {
            if (ModelState.IsValid)
            {
                _context.Etkinlikler.Add(etkinlik);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(etkinlik);
        }

        // 3. Etkinlik Güncelleme (GET)
        public IActionResult Edit(int id)
        {
            var etkinlik = _context.Etkinlikler.Find(id);
            if (etkinlik == null)
            {
                return NotFound();
            }
            return View(etkinlik);
        }

        // 3. Etkinlik Güncelleme (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Etkinlik etkinlik)
        {
            if (id != etkinlik.ID)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _context.Etkinlikler.Update(etkinlik);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(etkinlik);
        }

        // 4. Etkinlik Silme
        public IActionResult Delete(int id)
        {
            var etkinlik = _context.Etkinlikler.Find(id);
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
    }
}

