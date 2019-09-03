using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WeReview.Models
{
    public class GitHubLine
    {
        [Key]
        public int LineId { get; set; }
        public int LineInFile { get; set; }
        public string Content { get; set; }
        public bool IsChanged { get; set; }
        public bool IsApproved { get; set; }
        public int FileId { get; set; }
        [ForeignKey("FileId")]
        public GitHubFile File { get; set; }
    }
}
