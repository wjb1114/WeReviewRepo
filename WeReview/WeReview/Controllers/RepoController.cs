using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Octokit;
using WeReview.Data;
using WeReview.Models;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using Octokit.Internal;

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

            GetBranchData(repo);

            return View(repo);
        }

        private async void GetBranchData(GitHubRepository repo)
        {
            string AccessToken = await HttpContext.GetTokenAsync("access_token");

            string[] repoData = repo.FullName.Split('/');

            var github = new GitHubClient(new ProductHeaderValue("AspNetCoreGitHubAuth"),
                new InMemoryCredentialStore(new Credentials(AccessToken)));
            IReadOnlyList<Branch> branches = await github.Repository.Branch.GetAll(repoData[0], repoData[1]);
            foreach (Branch b in branches)
            {
                GitHubBranch branch;
                lock(thisLock)
                {
                    branch = _context.GitHubBranches.Where(br => br.Name == b.Name).Where(br => br.RepositoryId == repo.RepositoryId).SingleOrDefault();
                }
                if (branch == null)
                {
                    branch = new GitHubBranch();
                    branch.Name = b.Name;
                    lock (thisLock)
                    {
                        _context.GitHubBranches.Add(branch);
                        _context.SaveChanges();
                    }
                }
            }

        }
    }
}