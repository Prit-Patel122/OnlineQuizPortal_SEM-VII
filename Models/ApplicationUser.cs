using Microsoft.AspNetCore.Identity;

namespace OnlineQuizPortal.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin, Teacher, Student

        // Navigation properties
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
        public virtual ICollection<Result> Results { get; set; } = new List<Result>();
        public virtual ICollection<Quiz> CreatedQuizzes { get; set; } = new List<Quiz>();
    }
}
