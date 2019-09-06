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
        public bool MasterSelected { get; set; }
        [Range(0, 100, ErrorMessage = "Value for {0} must be between {1} and {2}")]
        public int ReviewThreshold { get; set; }
        public ICollection<GitHubUserRepository> UserRepositories { get; set; }
    }
}
