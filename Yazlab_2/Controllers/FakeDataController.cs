using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Yazlab_2.Models.EntityBase;
using Bogus;
using System;
using System.Linq;
using Yazlab_2.Data;
using Bogus.DataSets;

namespace Yazlab_2.Controllers
{
    public class FakeDataController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public FakeDataController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult CreateFakeData()
        {
          

        

            return Ok("Tüm fake veriler başarıyla oluşturuldu.");
        }
    }
}
