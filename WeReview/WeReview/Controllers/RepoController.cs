using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Octokit;
using WeReview.Data;
using WeReview.Models;

namespace WeReview.Controllers
{
    [Route("[controller]/[action]")]
    public class RepoController : Controller
    {
        private readonly object thisLock;
        private ApplicationDbContext _context;

        public RepoController(ApplicationDbContext context)
        {
            thisLock = new object();
            _context = context;
        }
        public IActionResult Index(int id)
        {
            GitHubRepository repo;
            lock (thisLock)
            {
                repo = _context.GitHubRepos.Where(r => r.RepositoryId == id).Single();
            }
            return View(repo);
        }
    }
}