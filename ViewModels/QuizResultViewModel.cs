using OnlineQuizPortal.Models;

namespace OnlineQuizPortal.ViewModels
{
    public class QuizResultViewModel
    {
        public Quiz Quiz { get; set; }
        public Result Result { get; set; }
        public List<UserAnswer> UserAnswers { get; set; }

        public QuizResultViewModel()
        {
            Quiz = new Quiz();
            Result = new Result();
            UserAnswers = new List<UserAnswer>();
        }
    }
}
