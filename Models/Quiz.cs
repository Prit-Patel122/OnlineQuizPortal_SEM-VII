using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace OnlineQuizPortal.Models
{
    [Index(nameof(Subject), nameof(Title))]
    [Index(nameof(CreatedBy))]
    public class Quiz
    {
        public int QuizId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [Range(1, 300, ErrorMessage = "Duration must be between 1 and 300 minutes")]
        public int Duration { get; set; } // Duration in minutes

        [Required]
        [Range(1, 1000, ErrorMessage = "Total marks must be between 1 and 1000")]
        public int TotalMarks { get; set; }

        public string CreatedBy { get; set; } = string.Empty; // UserId of creator
        public virtual ApplicationUser CreatedByUser { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
        public virtual ICollection<Result> Results { get; set; } = new List<Result>();
    }
}
