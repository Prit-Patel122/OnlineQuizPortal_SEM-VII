using System;

namespace OnlineQuizPortal.ViewModels
{
    public class QuizHistoryViewModel
    {
        public int ResultId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int AttemptedQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public double TimeTaken { get; set; }
        public bool IsCompleted { get; set; }
        public decimal Percentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
