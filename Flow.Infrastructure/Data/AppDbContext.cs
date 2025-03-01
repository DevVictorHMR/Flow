using Flow.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Flow.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> User => Set<User>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder); 

        //    modelBuilder.Entity<Feedback>(entity =>
        //    {
        //        entity.HasOne(f => f.Sender)
        //            .WithMany()
        //            .HasForeignKey(f => f.SenderId)
        //            .OnDelete(DeleteBehavior.NoAction); // <-- Desativa CASCADE

        //        entity.HasOne(f => f.Receiver)
        //            .WithMany()
        //            .HasForeignKey(f => f.ReceiverId)
        //            .OnDelete(DeleteBehavior.NoAction); // <-- Desativa CASCADE
        //    });
        //}
    }
}
