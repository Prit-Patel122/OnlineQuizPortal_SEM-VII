using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineQuizPortal.Models;
using System.Threading.Tasks;

namespace OnlineQuizPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public static readonly Microsoft.EntityFrameworkCore.QueryTrackingBehavior DefaultQueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<Quiz>()
                .HasOne(q => q.CreatedByUser)
                .WithMany(u => u.CreatedQuizzes)
                .HasForeignKey(q => q.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(q => q.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserAnswer>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserAnswers)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserAnswer>()
                .HasOne(ua => ua.Quiz)
                .WithMany(q => q.UserAnswers)
                .HasForeignKey(ua => ua.QuizId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany(q => q.UserAnswers)
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Result>()
                .HasOne(r => r.User)
                .WithMany(u => u.Results)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Result>()
                .HasOne(r => r.Quiz)
                .WithMany(q => q.Results)
                .HasForeignKey(r => r.QuizId)
                .OnDelete(DeleteBehavior.NoAction);

            // Disable cascade delete for UserAnswers
            builder.Entity<UserAnswer>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserAnswers)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<UserAnswer>()
                .HasOne(ua => ua.Quiz)
                .WithMany(q => q.UserAnswers)
                .HasForeignKey(ua => ua.QuizId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany(q => q.UserAnswers)
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure primary keys
            builder.Entity<Quiz>()
                .HasKey(q => q.QuizId);

            builder.Entity<Question>()
                .HasKey(q => q.QuestionId);

            builder.Entity<UserAnswer>()
                .HasKey(ua => ua.AnswerId);

            builder.Entity<Result>()
                .HasKey(r => r.ResultId);
        }
    }
}
