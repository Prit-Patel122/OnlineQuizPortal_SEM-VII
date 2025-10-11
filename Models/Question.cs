using System.ComponentModel.DataAnnotations;

namespace OnlineQuizPortal.Models
{
    public class Question
    {
        public int QuestionId { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        public string OptionA { get; set; } = string.Empty;

        [Required]
        public string OptionB { get; set; } = string.Empty;

        [Required]
        public string OptionC { get; set; } = string.Empty;

        [Required]
        public string OptionD { get; set; } = string.Empty;

        [Required]
        public string CorrectAnswer { get; set; } = string.Empty; // A, B, C, or D

        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}
