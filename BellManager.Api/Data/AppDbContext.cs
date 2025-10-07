using BellManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BellManager.Api.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		public DbSet<Alarm> Alarms => Set<Alarm>();

		public DbSet<User> Users => Set<User>();

		public DbSet<Church> Churches => Set<Church>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Alarm>(entity =>
			{
				entity.ToTable("alarms");
				entity.HasKey(a => a.Id);
				entity.Property(a => a.Id).HasColumnName("id");
				entity.Property(a => a.UserId).HasColumnName("user_id");
				entity.Property(a => a.BellName).IsRequired().HasMaxLength(200).HasColumnName("bell_name");
				entity.Property(a => a.HourUtc)
					.HasConversion(
						v => v.ToString("HH:mm"),
						v => TimeOnly.Parse(v))
					.HasColumnName("hour_utc");
				entity.Property(a => a.DaysOfWeek).HasMaxLength(64).HasColumnName("days_of_week");
				entity.Property(a => a.IsEnabled).HasColumnName("is_enabled");
				entity.Property(a => a.Notes).HasMaxLength(1000).HasColumnName("notes");
				
				// New fields
				entity.Property(a => a.RepeatType).IsRequired().HasMaxLength(32).HasColumnName("repeat_type").HasDefaultValue("Once");
				entity.Property(a => a.IsRepeating).HasColumnName("is_repeating").HasDefaultValue(false);
				entity.Property(a => a.SelectedDate).HasColumnName("selected_date");
				entity.Property(a => a.ChurchId).HasColumnName("church_id");
				
				entity.HasOne(a => a.User)
					.WithMany(u => u.Alarms)
					.HasForeignKey(a => a.UserId)
					.OnDelete(DeleteBehavior.Cascade);
					
				entity.HasOne(a => a.Church)
					.WithMany(c => c.Alarms)
					.HasForeignKey(a => a.ChurchId)
					.OnDelete(DeleteBehavior.SetNull);
			});

			modelBuilder.Entity<User>(entity =>
			{
				entity.ToTable("users");
				entity.HasKey(u => u.Id);
				entity.Property(u => u.Id).HasColumnName("id");
				entity.Property(u => u.Email).IsRequired().HasMaxLength(256).HasColumnName("email");
				entity.Property(u => u.UserName).IsRequired().HasMaxLength(64).HasColumnName("username");
				entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512).HasColumnName("password_hash");
				entity.Property(u => u.Role).IsRequired().HasMaxLength(32).HasColumnName("role");
				entity.Property(u => u.ChurchId).HasColumnName("church_id");
				entity.HasIndex(u => u.Email).IsUnique();
				entity.HasIndex(u => u.UserName).IsUnique();
			});

			modelBuilder.Entity<Church>(entity =>
			{
				entity.ToTable("churches");
				entity.HasKey(c => c.Id);
				entity.Property(c => c.Id).HasColumnName("id");
				entity.Property(c => c.Name).IsRequired().HasMaxLength(200).HasColumnName("name");
				entity.Property(c => c.PhoneNumber).HasMaxLength(32).HasColumnName("phone_number");
			});

			modelBuilder.Entity<User>()
				.HasOne(u => u.Church)
				.WithMany(c => c.Users)
				.HasForeignKey(u => u.ChurchId)
				.OnDelete(DeleteBehavior.SetNull);

			base.OnModelCreating(modelBuilder);
		}
	}
}


