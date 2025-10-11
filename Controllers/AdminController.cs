using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineQuizPortal.Data;
using OnlineQuizPortal.Models;

namespace OnlineQuizPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalQuizzes = await _context.Quizzes.CountAsync();
            var totalQuestions = await _context.Questions.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalResults = await _context.Results.CountAsync();

            var recentResults = await _context.Results
                .Include(r => r.User)
                .Include(r => r.Quiz)
                .OrderByDescending(r => r.StartedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.TotalQuizzes = totalQuizzes;
            ViewBag.TotalQuestions = totalQuestions;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalResults = totalResults;
            ViewBag.RecentResults = recentResults;

            return View();
        }

        public async Task<IActionResult> AllResults()
        {
            var results = await _context.Results
                .Include(r => r.User)
                .Include(r => r.Quiz)
                .OrderByDescending(r => r.StartedAt)
                .ToListAsync();

            return View(results);
        }

        public async Task<IActionResult> StudentPerformance()
        {
            var studentPerformance = await _context.Users
                .Where(u => u.Role == "Student")
                .Select(u => new
                {
                    User = u,
                    TotalAttempts = _context.Results.Count(r => r.UserId == u.Id),
                    AverageScore = _context.Results
                        .Where(r => r.UserId == u.Id && r.IsCompleted)
                        .Average(r => (double?)r.Score) ?? 0,
                    CompletedQuizzes = _context.Results.Count(r => r.UserId == u.Id && r.IsCompleted)
                })
                .OrderByDescending(sp => sp.AverageScore)
                .ToListAsync();

            ViewBag.StudentPerformance = studentPerformance;
            return View();
        }

        public async Task<IActionResult> QuizStatistics()
        {
            var quizStats = await _context.Quizzes
                .Select(q => new ViewModels.QuizStatisticsViewModel
                {
                    Quiz = q,
                    TotalAttempts = _context.Results.Count(r => r.QuizId == q.QuizId),
                    AverageScore = _context.Results
                        .Where(r => r.QuizId == q.QuizId && r.IsCompleted)
                        .Average(r => (double?)r.Score) ?? 0,
                    CompletionRate = _context.Results.Count(r => r.QuizId == q.QuizId && r.IsCompleted) * 100.0 /
                        Math.Max(_context.Results.Count(r => r.QuizId == q.QuizId), 1)
                })
                .OrderByDescending(qs => qs.TotalAttempts)
                .ToListAsync();

            // Pre-calculate chart data for the view
            var titles = quizStats.Select(s => s.Quiz.Title).ToList();
            var attempts = quizStats.Select(s => s.TotalAttempts).ToList();
            var scores = quizStats.Select(s => s.AverageScore).ToList();
            var completion = quizStats.Select(s => s.CompletionRate).ToList();
            var subjectStats = quizStats
                .GroupBy(q => q.Quiz.Subject)
                .Select(g => new ViewModels.SubjectStatisticsViewModel
                {
                    Subject = g.Key,
                    AverageScore = g.Average(q => q.AverageScore),
                    TotalAttempts = g.Sum(q => q.TotalAttempts)
                })
                .ToList();

            ViewBag.QuizStats = quizStats;
            ViewBag.Titles = titles;
            ViewBag.Attempts = attempts;
            ViewBag.Scores = scores;
            ViewBag.Completion = completion;
            ViewBag.SubjectStats = subjectStats;
            return View();
        }
    }
}
