using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebDevEsports.Models;
using WebDevEsports.Data;
using Microsoft.EntityFrameworkCore;

namespace WebDevEsports.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: List of announcements
        public async Task<IActionResult> Index()
        {
            List<Announcement> announcements = await _context.Announcement.OrderByDescending(a => a.DateTime).ToListAsync();
            List<Announcement> latestNews = announcements.GetRange(0, 3);
            HomeIndexViewModel viewModel = new HomeIndexViewModel
            {
                LatestNews = latestNews
            };
            return View(viewModel);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Stream()
        {
            return View();
        }

        public IActionResult Store()
        {
            return Redirect("https://shop.eslgaming.com/collections/g2esports");
        }
    }
}
