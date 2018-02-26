using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebDevEsports.Data;
using WebDevEsports.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace WebDevEsports.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHostingEnvironment _environment;

        public AnnouncementsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHostingEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: Announcements
        public async Task<IActionResult> Index()
        {
            return View(await _context.Announcement.OrderByDescending(a => a.DateTime).ToListAsync());
        }

        // GET: Announcements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Announcement announcement = await _context.Announcement
                .SingleOrDefaultAsync(m => m.Id == id);
            if (announcement == null)
            {
                return NotFound();
            }
            announcement.NumberViews += 1;
            _context.Announcement.Update(announcement);
            await _context.SaveChangesAsync();

            AnnouncementCommentsViewModel viewModel = await GetAnnouncementCommentsViewModelFromAnnouncement(announcement);

            return View(viewModel);
        }

        // POST: Announcements/Details/5
        // Add a comment to the parent announcement
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Details([Bind("AnnouncementID", "Comment")] AnnouncementCommentsViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
                // Create a new comment using data from viewModel
                Comment comment = new Comment
                {
                    Content = viewModel.Comment,
                    DateTime = DateTime.Now,
                    AuthorDisplayName = user.DisplayName,
                    Author = user
                };

                // Find the referenced announcement in the database
                Announcement announcement = await _context.Announcement
                    .SingleOrDefaultAsync(m => m.Id == viewModel.AnnouncementID);
                if (announcement == null)
                {
                    return NotFound();
                }

                // Assign the announcement to the comment
                comment.MyAnnouncement = announcement;
                // Add the comment to the database and save
                _context.Comment.Add(comment);
                await _context.SaveChangesAsync();

                viewModel = await GetAnnouncementCommentsViewModelFromAnnouncement(announcement);
            }


            return View(viewModel);
        }

        // Takes an announcement and extracts all the corresponding comments and adds them to the view model
        private async Task<AnnouncementCommentsViewModel> GetAnnouncementCommentsViewModelFromAnnouncement(Announcement announcement)
        {
            AnnouncementCommentsViewModel viewModel = new AnnouncementCommentsViewModel
            {
                Announcement = announcement,
                AnnouncementID = announcement.Id,
                Content = announcement.Content,
                DateTime = announcement.DateTime,
                AuthorDisplayName = announcement.AuthorDisplayName,
                NumberViews = announcement.NumberViews
            };

            List<Comment> comments = await _context.Comment
                .Where(m => m.MyAnnouncement == announcement).ToListAsync();

            viewModel.Comments = comments;
            
            return viewModel;
        }

        // GET: Announcements/Create
        [Authorize(Roles="Member")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Announcements/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,DateTime")] Announcement announcement, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                if (file != null && file.Length > 0)
                {
                    announcement.ImageName = await FileManager.UploadFile(_environment, file);
                }

                ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
                announcement.DateTime = DateTime.Now;
                announcement.Author = user;
                announcement.AuthorDisplayName = user.DisplayName;
                _context.Add(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        // GET: Announcements/Edit/5
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Announcement announcement = await _context.Announcement.SingleOrDefaultAsync(m => m.Id == id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        // POST: Announcements/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,DateTime,AuthorDisplayName,Author,ImageName,NumberViews")] Announcement announcement)
        {
            if (id != announcement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(announcement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnnouncementExists(announcement.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        // GET: Announcements/Delete/5
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Announcement announcement = await _context.Announcement
                .SingleOrDefaultAsync(m => m.Id == id);
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // POST: Announcements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the selected announcement and corresponding comments
            Announcement announcement = await _context.Announcement.SingleOrDefaultAsync(m => m.Id == id);
            List<Comment> comments = await _context.Comment
                .Where(m => m.MyAnnouncement == announcement).ToListAsync();

            // Remove announcement and comments from db
            _context.Announcement.Remove(announcement);
            foreach (Comment c in comments)
            {
                _context.Comment.Remove(c);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Announcements/Details/DeleteComment/5
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> DeleteComment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Comment comment = await _context.Comment
                .SingleOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Announcements/Details/DeleteComment/5
        [HttpPost, ActionName("DeleteComment")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            // Find the selected announcement and corresponding comments
            Comment comment = await _context.Comment.SingleOrDefaultAsync(m => m.Id == id);

            // Remove announcement and comments from db
            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnnouncementExists(int id)
        {
            return _context.Announcement.Any(e => e.Id == id);
        }


    }
}
