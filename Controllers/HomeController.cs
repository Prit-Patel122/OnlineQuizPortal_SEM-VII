using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineQuizPortal.Data;
using OnlineQuizPortal.Models;

namespace OnlineQuizPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    
                    if (currentUser?.Role == "Admin" || currentUser?.Role == "Teacher")
                    {
                        var quizzes = await _context.Quizzes
                            .Include(q => q.CreatedByUser)
                            .Include(q => q.Questions)
                            .Where(q => q.CreatedBy == currentUser.Id)
                            .OrderByDescending(q => q.QuizId)
                            .Take(5)
                            .ToListAsync();

                        ViewBag.Quizzes = quizzes ?? new List<Quiz>();
                        ViewBag.UserRole = currentUser.Role;
                        ViewBag.UserName = currentUser.Name;
                        return View("AdminDashboard");
                    }
                    else if (currentUser?.Role == "Student")
                    {
                        var availableQuizzes = await _context.Quizzes
                            .Include(q => q.CreatedByUser)
                            .OrderBy(q => q.Subject)
                            .ThenBy(q => q.Title)
                            .Take(5)
                            .ToListAsync();

                        var recentResults = await _context.Results
                            .Include(r => r.Quiz)
                            .Where(r => r.UserId == currentUser.Id && r.IsCompleted)
                            .OrderByDescending(r => r.CompletedAt)
                            .Take(5)
                            .ToListAsync();

                        ViewBag.AvailableQuizzes = availableQuizzes ?? new List<Quiz>();
                        ViewBag.RecentResults = recentResults ?? new List<Result>();
                        ViewBag.UserRole = currentUser.Role;
                        ViewBag.UserName = currentUser.Name;
                        return View("StudentDashboard");
                    }
                }

                return View();
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                Console.WriteLine($"HomeController error: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred. Please try again.";
                return View();
            }
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> AdminDashboard()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var quizzes = await _context.Quizzes
                    .Include(q => q.CreatedByUser)
                    .Include(q => q.Questions)
                    .Where(q => q.CreatedBy == currentUser.Id)
                    .OrderByDescending(q => q.QuizId)
                    .ToListAsync();

                ViewBag.Quizzes = quizzes ?? new List<Quiz>();
                ViewBag.UserRole = currentUser.Role;
                ViewBag.UserName = currentUser.Name;
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading dashboard. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentDashboard()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var availableQuizzes = await _context.Quizzes
                    .Include(q => q.CreatedByUser)
                    .OrderBy(q => q.Subject)
                    .ThenBy(q => q.Title)
                    .ToListAsync();

                var recentResults = await _context.Results
                    .Include(r => r.Quiz)
                    .Where(r => r.UserId == currentUser.Id)
                    .OrderByDescending(r => r.StartedAt)
                    .ToListAsync();

                ViewBag.AvailableQuizzes = availableQuizzes ?? new List<Quiz>();
                ViewBag.RecentResults = recentResults ?? new List<Result>();
                ViewBag.UserRole = currentUser.Role;
                ViewBag.UserName = currentUser.Name;
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading dashboard. Please try again.";
                return RedirectToAction("Index");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
