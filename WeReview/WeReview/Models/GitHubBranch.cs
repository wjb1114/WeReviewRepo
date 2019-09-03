using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WeReview.Models
{
    public class GitHubBranch
    {
        [Key]
        public int BranchId { get; set; }
        public string Name { get; set; }
        public string Sha { get; set; }
        public string ApiUrl { get; set; }
        public bool IsMaster { get; set; }
        public int RepositoryId { get; set; }
        [ForeignKey("RepositoryId")]
        public GitHubRepository Repository { get; set; }
    }
}
