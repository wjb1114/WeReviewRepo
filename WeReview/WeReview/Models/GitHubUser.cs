using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WeReview.Models
{
    public class GitHubUser
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string GitHubUrl { get; set; }
        public string GitHubUserId { get; set; }
        public ICollection<GitHubUserRepository> UserRepositories { get; set; }
    }
}
