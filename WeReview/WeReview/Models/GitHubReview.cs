using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WeReview.Models
{
    public class GitHubReview
    {
        [Key]
        public int Id { get; set; }
        public int[] LineIds { get; set; }
        public string Comment { get; set; }
        public string Reason { get; set; }
        public int FileId { get; set; }
        [ForeignKey("FileId")]
        public GitHubFile File { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public GitHubUser User { get; set; }
        public bool IsApproved { get; set; }
    }
}
