using System.ComponentModel.DataAnnotations;

namespace OnlineQuizPortal.Models
{
    public class UserAnswer
    {
        public int AnswerId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; } = null!;

        [Required]
        public int QuestionId { get; set; }
        public virtual Question Question { get; set; } = null!;

        [Required]
        public string SelectedOption { get; set; } = string.Empty; // A, B, C, or D

        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    }
}
