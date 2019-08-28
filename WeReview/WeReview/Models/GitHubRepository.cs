using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WeReview.Models
{
    public class GitHubRepository
    {
        [Key]
        public int RepositoryId { get; set; }
        public string CloneUrl { get; set; }
        public string FullName { get; set; }
        public string ApiUrl { get; set; }
        public ICollection<GitHubUserRepository> UserRepositories { get; set; }
    }
}
