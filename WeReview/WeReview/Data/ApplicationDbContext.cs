using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WeReview.Models;

namespace WeReview.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<GitHubUser> GitHubUsers { get; set; }
        public DbSet<GitHubRepository> GitHubRepos { get; set; }
        public DbSet<GitHubBranch> GitHubBranches { get; set; }
        public DbSet<GitHubFile> GitHubFiles { get; set; }
        public DbSet<GitHubLine> GitHubLines { get; set; }
        public DbSet<GitHubUserRepository> GitHubUserRepos { get; set; }
        public DbSet<GitHubReview> GitHubReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ValueConverter<int[], string> reviewConverter = new ValueConverter<int[], string>(
                v => string.Join(";", v),
                v => v.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(val => int.Parse(val)).ToArray());

            builder.Entity<GitHubUser>().HasKey(x => x.UserId);
            builder.Entity<GitHubRepository>().HasKey(x => x.RepositoryId);
            builder.Entity<GitHubUserRepository>().HasKey(x => new { x.UserId, x.RepositoryId });
            builder.Entity<GitHubUserRepository>().HasOne(x => x.Repository).WithMany(x => x.UserRepositories).HasForeignKey(x => x.RepositoryId);
            builder.Entity<GitHubUserRepository>().HasOne(x => x.User).WithMany(x => x.UserRepositories).HasForeignKey(x => x.UserId);

            builder.Entity<GitHubReview>().Property(e => e.LineIds).HasConversion(reviewConverter);
            base.OnModelCreating(builder);
        }
    }
}
