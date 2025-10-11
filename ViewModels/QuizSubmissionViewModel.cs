using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineQuizPortal.ViewModels
{
    public class QuizSubmissionViewModel
    {
        public int QuizId { get; set; }
        public DateTime StartedAt { get; set; }
        public List<QuestionSubmission> Questions { get; set; } = new();
    }

    public class QuestionSubmission
    {
        public int QuestionId { get; set; }
        [Required(ErrorMessage = "Please select an answer")]
        public string SelectedAnswer { get; set; }
    }
}
