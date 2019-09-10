using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WeReview.Models
{
    public class CsNode
    {
        [Key]
        public int Id { get; set; }
        public string NodeName { get; set; }
        public List<int> LineIds { get; set; }
        public int FileId { get; set; }
        [ForeignKey("FileId")]
        public GitHubFile File { get; set; }
        public int? ParentNodeId { get; set; }
        public List<int> ChildNodeIds { get; set; }
        [ForeignKey("ParentNodeId")]
        public CsNode ParentNode { get; set; }
    }
}
