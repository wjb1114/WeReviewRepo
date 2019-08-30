using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WeReview.Models
{
    public class GitHubFile
    {
        [Key]
        public int FileId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string DownloadPath { get; set; }
        public bool IsFile { get; set; }
        public bool IsDir { get; set; }
        public int ParentId { get; set; }
        public int BranchId { get; set; }
        [ForeignKey("BranchId")]
        public GitHubBranch Branch { get; set; }
    }
}
