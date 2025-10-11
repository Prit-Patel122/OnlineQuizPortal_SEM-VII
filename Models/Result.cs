using System.ComponentModel.DataAnnotations;

namespace OnlineQuizPortal.Models
{
    public class Result
    {
        public int ResultId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; } = null!;

        [Required]
        [Range(0, 1000)]
        public int Score { get; set; }

        [Required]
        public DateTime AttemptDate { get; set; } = DateTime.UtcNow;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; } = false;
    }
}
