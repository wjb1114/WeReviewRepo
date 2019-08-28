using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeReview.Models
{
    public class GitHubUserRepository
    {
        public int UserId { get; set; }
        public GitHubUser User { get; set; }

        public int RepositoryId { get; set; }
        public GitHubRepository Repository { get; set; }
    }
}
