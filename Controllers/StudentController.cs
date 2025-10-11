using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineQuizPortal.Data;
using OnlineQuizPortal.Models;
using OnlineQuizPortal.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Antiforgery;

namespace OnlineQuizPortal.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAntiforgery _antiforgery;

        public StudentController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IAntiforgery antiforgery)
        {
            _context = context;
            _userManager = userManager;
            _antiforgery = antiforgery;
        }

        public async Task<IActionResult> Index()
        {
            var availableQuizzes = await _context.Quizzes
                .Include(q => q.CreatedByUser)
                .OrderBy(q => q.Subject)
                .ThenBy(q => q.Title)
                .ToListAsync();

            return View(availableQuizzes);
        }

        public async Task<IActionResult> Attempt(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user has an incomplete attempt
            // Check if user has an incomplete attempt
            var existingResult = await _context.Results
                .FirstOrDefaultAsync(r => r.UserId == currentUser.Id && r.QuizId == id && !r.IsCompleted);

            if (existingResult == null)
            {
                // Create new attempt
                existingResult = new Result
                {
                    UserId = currentUser.Id,
                    QuizId = id,
                    Score = 0,
                    AttemptDate = DateTime.UtcNow,
                    StartedAt = DateTime.UtcNow,
                    IsCompleted = false
                };
                _context.Results.Add(existingResult);
                await _context.SaveChangesAsync();
            }

            var model = new QuizAttemptViewModel
            {
                QuizId = quiz.QuizId,
                QuizTitle = quiz.Title,
                Subject = quiz.Subject,
                Duration = quiz.Duration,
                TotalMarks = quiz.TotalMarks,
                StartedAt = existingResult.StartedAt,
                Questions = quiz.Questions.Select(q => new QuestionAttemptViewModel
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    SelectedAnswer = ""
                }).ToList()
            };

            ViewBag.ResultId = existingResult.ResultId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(QuizAttemptViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check your answers and try again.";
                return RedirectToAction(nameof(Attempt), new { id = model.QuizId });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {

                // Get the quiz and questions
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.QuizId == model.QuizId);

                if (quiz == null)
                {
                    TempData["Error"] = "Quiz not found.";
                    return RedirectToAction(nameof(Attempt), new { id = model.QuizId });
                }

                // Get or create result
                var result = await _context.Results
                    .FirstOrDefaultAsync(r => r.QuizId == model.QuizId && r.UserId == currentUser.Id && !r.IsCompleted);

                if (result == null)
                {
                    TempData["Error"] = "Quiz attempt not found.";
                    return RedirectToAction(nameof(Attempt), new { id = model.QuizId });
                }

                // Check if quiz is already completed
                if (result.IsCompleted)
                {
                    TempData["Error"] = "This quiz has already been submitted.";
                    return RedirectToAction(nameof(Result), new { resultId = result.ResultId });
                }

                // Check if quiz time has expired
                var timeTaken = DateTime.UtcNow - result.StartedAt;
                var timeAllowed = TimeSpan.FromMinutes(quiz.Duration);
                
                if (timeTaken > timeAllowed)
                {
                    var questions = quiz.Questions.ToList();
                    var existingAnswers = await _context.UserAnswers
                        .Where(ua => ua.UserId == currentUser.Id && ua.QuizId == result.QuizId)
                        .ToListAsync();

                    // Calculate the score for answered questions before time expired
                    decimal marksPerQuestion = quiz.TotalMarks / (decimal)questions.Count;
                    int answeredBeforeTimeout = 0;

                    foreach (var answer in model.Questions)
                    {
                        var question = questions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
                        var userAnswer = existingAnswers.FirstOrDefault(ua => ua.QuestionId == answer.QuestionId);
                        
                        if (question != null && userAnswer != null && 
                            userAnswer.AnsweredAt - result.StartedAt <= timeAllowed)
                        {
                            if (answer.SelectedAnswer == question.CorrectAnswer)
                            {
                                answeredBeforeTimeout += (int)Math.Round(marksPerQuestion, 0, MidpointRounding.AwayFromZero);
                            }
                        }
                    }

                    // Update result with timeout score
                    result.Score = Math.Min(answeredBeforeTimeout, quiz.TotalMarks);
                    result.CompletedAt = result.StartedAt.Add(timeAllowed);
                    result.IsCompleted = true;
                    await _context.SaveChangesAsync();

                    TempData["Warning"] = "Quiz time expired. Only answers submitted within the time limit were counted.";
                    return RedirectToAction(nameof(Result), new { resultId = result.ResultId });
                }

                // Calculate score and save answers
                int score = 0;
                int totalScore = 0;
                var allQuestions = quiz.Questions.ToList();

                // Clear any existing answers for this attempt
                var allExistingAnswers = await _context.UserAnswers
                    .Where(ua => ua.UserId == currentUser.Id && ua.QuizId == result.QuizId)
                    .ToListAsync();
                if (allExistingAnswers.Any())
                {
                    _context.UserAnswers.RemoveRange(allExistingAnswers);
                }

                // Calculate marks per question
                decimal questionMarks = quiz.TotalMarks / (decimal)allQuestions.Count;

                // Process each answer
                foreach (var answer in model.Questions)
                {
                    var question = allQuestions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
                    if (question != null)
                    {
                        // Check if answer is correct
                        if (answer.SelectedAnswer == question.CorrectAnswer)
                        {
                            totalScore += (int)Math.Round(questionMarks, 0, MidpointRounding.AwayFromZero);
                        }

                        // Save user answer
                        var userAnswer = new UserAnswer
                        {
                            UserId = currentUser.Id,
                            QuizId = model.QuizId,
                            QuestionId = question.QuestionId,
                            SelectedOption = answer.SelectedAnswer,
                            AnsweredAt = DateTime.UtcNow
                        };
                        _context.UserAnswers.Add(userAnswer);
                    }
                }

                // Ensure total score doesn't exceed quiz total marks
                score = Math.Min(totalScore, quiz.TotalMarks);

                // Update result
                result.Score = score;
                result.CompletedAt = DateTime.UtcNow;
                result.IsCompleted = true;

                await _context.SaveChangesAsync();

                // Calculate percentage
                decimal percentage = (score * 100M) / quiz.TotalMarks;
                TempData["Success"] = $"Quiz completed successfully! Your score: {score}/{quiz.TotalMarks} ({percentage:F1}%)";
                return RedirectToAction(nameof(Result), new { resultId = result.ResultId });
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error submitting quiz: {ex.Message}");
                TempData["Error"] = "An error occurred while submitting your quiz. Please try again.";
                return RedirectToAction(nameof(Attempt), new { id = model.QuizId });
            }
        }

        public async Task<IActionResult> Result(int resultId)
        {
            var result = await _context.Results
                .Include(r => r.Quiz)
                    .ThenInclude(q => q.Questions)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ResultId == resultId);

            if (result == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (result.UserId != currentUser.Id)
            {
                return Forbid();
            }

            // Get user's answers for this quiz
            var userAnswers = await _context.UserAnswers
                .Where(ua => ua.UserId == currentUser.Id && ua.QuizId == result.QuizId)
                .ToListAsync();

            var viewModel = new QuizResultViewModel
            {
                Quiz = result.Quiz,
                Result = result,
                UserAnswers = userAnswers
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExitQuiz(int quizId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            try
            {
                var result = await _context.Results
                    .FirstOrDefaultAsync(r => r.QuizId == quizId && r.UserId == currentUser.Id && !r.IsCompleted);

                if (result == null)
                {
                    return Json(new { success = false, message = "Quiz attempt not found." });
                }

                // Mark the quiz as abandoned
                result.IsCompleted = true;
                result.CompletedAt = DateTime.UtcNow;
                result.Score = 0;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Quiz has been exited",
                    redirectUrl = Url.Action("Index", "Student")
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while exiting the quiz." });
            }
        }

        public async Task<IActionResult> History()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var results = await _context.Results
                .Include(r => r.Quiz)
                    .ThenInclude(q => q.Questions)
                .Where(r => r.UserId == currentUser.Id)
                .OrderByDescending(r => r.StartedAt)
                .Select(r => new QuizHistoryViewModel
                {
                    ResultId = r.ResultId,
                    QuizId = r.QuizId,
                    QuizTitle = r.Quiz.Title,
                    Subject = r.Quiz.Subject,
                    TotalQuestions = r.Quiz.Questions.Count,
                    Score = r.Score,
                    TotalMarks = r.Quiz.TotalMarks,
                    StartedAt = r.StartedAt,
                    CompletedAt = r.CompletedAt,
                    TimeTaken = r.CompletedAt.HasValue ? (r.CompletedAt.Value - r.StartedAt).TotalMinutes : 0,
                    IsCompleted = r.IsCompleted,
                    AttemptedQuestions = _context.UserAnswers.Count(ua => ua.QuizId == r.QuizId && ua.UserId == r.UserId),
                    CorrectAnswers = _context.UserAnswers.Count(ua => 
                        ua.QuizId == r.QuizId && 
                        ua.UserId == r.UserId && 
                        ua.SelectedOption == r.Quiz.Questions.FirstOrDefault(q => q.QuestionId == ua.QuestionId).CorrectAnswer)
                })
                .ToListAsync();

            foreach (var result in results)
            {
                result.Percentage = result.TotalMarks > 0 ? (result.Score * 100.0M) / result.TotalMarks : 0;
                result.Status = GetQuizStatus(result);
            }

            return View(results);
        }

        private string GetQuizStatus(QuizHistoryViewModel result)
        {
            if (!result.IsCompleted)
                return "In Progress";
            
            if (result.Percentage >= 80)
                return "Excellent";
            else if (result.Percentage >= 60)
                return "Good";
            else if (result.Percentage >= 40)
                return "Pass";
            else
                return "Needs Improvement";
        }
    }
}
