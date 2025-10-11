using System.ComponentModel.DataAnnotations;

namespace OnlineQuizPortal.ViewModels
{
    public class QuizViewModel
    {
        public int QuizId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        [Display(Name = "Quiz Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Subject cannot be longer than 100 characters")]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [Range(1, 300, ErrorMessage = "Duration must be between 1 and 300 minutes")]
        [Display(Name = "Duration (minutes)")]
        public int Duration { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Total marks must be between 1 and 1000")]
        [Display(Name = "Total Marks")]
        public int TotalMarks { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one question is required")]
        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
    }

    public class QuestionViewModel
    {
        public int QuestionId { get; set; }

        [Required]
        [Display(Name = "Question Text")]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Option A")]
        public string OptionA { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Option B")]
        public string OptionB { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Option C")]
        public string OptionC { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Option D")]
        public string OptionD { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Correct Answer")]
        public string CorrectAnswer { get; set; } = string.Empty;
    }
}
