using System.ComponentModel.DataAnnotations;

namespace OnlineQuizPortal.ViewModels
{
    public class QuizAttemptViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int TotalMarks { get; set; }
        public DateTime StartedAt { get; set; }
        public List<QuestionAttemptViewModel> Questions { get; set; } = new List<QuestionAttemptViewModel>();
    }

    public class QuestionAttemptViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string SelectedAnswer { get; set; } = string.Empty;
    }
}
