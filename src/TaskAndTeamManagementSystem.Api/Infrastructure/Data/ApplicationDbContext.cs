using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskAndTeamManagementSystem.Api.Domain.Entities;
using Task = TaskAndTeamManagementSystem.Api.Domain.Entities.Task;

namespace TaskAndTeamManagementSystem.Api.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Team> Teams { get; set; }
    public DbSet<Task> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Role).HasConversion<int>();
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<int>();

            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignToUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(e => e.AssignToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Team)
                .WithMany(t => t.Tasks)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Team>()
            .HasMany(t => t.Members)
            .WithMany(u => u.Teams)
            .UsingEntity<Dictionary<string, object>>(
                "TeamUser",
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                j => j.HasOne<Team>().WithMany().HasForeignKey("TeamId"),
                j => j.HasKey("TeamId", "UserId"));
    }
}

