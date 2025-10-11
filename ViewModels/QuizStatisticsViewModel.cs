using OnlineQuizPortal.Models;

namespace OnlineQuizPortal.ViewModels
{
    public class QuizStatisticsViewModel
    {
        public Quiz Quiz { get; set; }
        public int TotalAttempts { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
    }

    public class SubjectStatisticsViewModel
    {
        public string Subject { get; set; }
        public double AverageScore { get; set; }
        public int TotalAttempts { get; set; }
    }
}
