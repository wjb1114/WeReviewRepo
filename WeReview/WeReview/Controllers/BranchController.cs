using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeReview.Data;
using WeReview.Models;

namespace WeReview.Controllers
{
    [Route("[controller]/[action]")]
    public class BranchController : Controller
    {
        private readonly object thisLock;
        private ApplicationDbContext _context;

        public BranchController(ApplicationDbContext context)
        {
            _context = context;
            thisLock = new object();
        }
        public IActionResult Index(int id, int parentId = -1)
        {
            GitHubBranch thisBranch;
            List<GitHubFile> branchFiles;
            lock(thisLock)
            {
                thisBranch = _context.GitHubBranches.Where(b => b.BranchId == id).Single();
                branchFiles = _context.GitHubFiles.Where(f => f.BranchId == thisBranch.BranchId).Where(f => f.ParentId == parentId).ToList();
            }
            if (branchFiles.Count < 1 && parentId != -1)
            {
                lock (thisLock)
                {
                    branchFiles = _context.GitHubFiles.Where(f => f.BranchId == thisBranch.BranchId).Where(f => f.ParentId == -1).ToList();
                }
            }
            return View(branchFiles);
        }
    }
}