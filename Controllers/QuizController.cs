using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineQuizPortal.Data;
using OnlineQuizPortal.Models;
using OnlineQuizPortal.ViewModels;

namespace OnlineQuizPortal.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public QuizController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMemoryCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            string cacheKey = $"UserQuizzes_{currentUser.Id}";
            if (!_cache.TryGetValue(cacheKey, out List<Quiz> quizzes))
            {
                quizzes = await _context.Quizzes.AsNoTracking()
                    .Include(q => q.CreatedByUser)
                    .Include(q => q.Questions)
                    .Where(q => q.CreatedBy == currentUser.Id)
                    .OrderByDescending(q => q.QuizId)
                    .ToListAsync();

                _cache.Set(cacheKey, quizzes, _cacheDuration);
            }

            return View(quizzes);
        }

        public IActionResult Create()
        {
            var model = new QuizViewModel
            {
                Questions = new List<QuestionViewModel> { new QuestionViewModel() }
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuizViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                
                var quiz = new Quiz
                {
                    Title = model.Title,
                    Subject = model.Subject,
                    Duration = model.Duration,
                    TotalMarks = model.TotalMarks,
                    CreatedBy = currentUser.Id
                };

                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();

                foreach (var questionModel in model.Questions)
                {
                    var question = new Question
                    {
                        QuestionText = questionModel.QuestionText,
                        OptionA = questionModel.OptionA,
                        OptionB = questionModel.OptionB,
                        OptionC = questionModel.OptionC,
                        OptionD = questionModel.OptionD,
                        CorrectAnswer = questionModel.CorrectAnswer,
                        QuizId = quiz.QuizId
                    };
                    _context.Questions.Add(question);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Quiz created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (quiz.CreatedBy != currentUser.Id)
            {
                return Forbid();
            }

            var model = new QuizViewModel
            {
                QuizId = quiz.QuizId,
                Title = quiz.Title,
                Subject = quiz.Subject,
                Duration = quiz.Duration,
                TotalMarks = quiz.TotalMarks,
                Questions = quiz.Questions.Select(q => new QuestionViewModel
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    CorrectAnswer = q.CorrectAnswer
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuizViewModel model)
        {
            if (id != model.QuizId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.QuizId == id);

                if (quiz == null)
                {
                    return NotFound();
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (quiz.CreatedBy != currentUser.Id)
                {
                    return Forbid();
                }

                quiz.Title = model.Title;
                quiz.Subject = model.Subject;
                quiz.Duration = model.Duration;
                quiz.TotalMarks = model.TotalMarks;

                // Remove existing questions
                _context.Questions.RemoveRange(quiz.Questions);

                // Add new questions
                foreach (var questionModel in model.Questions)
                {
                    var question = new Question
                    {
                        QuestionText = questionModel.QuestionText,
                        OptionA = questionModel.OptionA,
                        OptionB = questionModel.OptionB,
                        OptionC = questionModel.OptionC,
                        OptionD = questionModel.OptionD,
                        CorrectAnswer = questionModel.CorrectAnswer,
                        QuizId = quiz.QuizId
                    };
                    _context.Questions.Add(question);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Quiz updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.CreatedByUser)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (quiz.CreatedBy != currentUser.Id)
            {
                return Forbid();
            }

            return View(quiz);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (quiz.CreatedBy != currentUser.Id)
            {
                return Forbid();
            }

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Quiz deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult AddQuestion([FromBody] QuestionViewModel question)
        {
            return PartialView("_QuestionPartial", question);
        }
    }
}
