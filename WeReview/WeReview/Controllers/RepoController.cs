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
        public async Task<IActionResult> Index(int id)
        {
            GitHubRepository repo;
            lock (thisLock)
            {
                repo = _context.GitHubRepos.Where(r => r.RepositoryId == id).Single();
            }

            await GetBranchData(repo);

            return View(repo);
        }

        private async Task GetBranchData(GitHubRepository repo)
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
                    branch.RepositoryId = repo.RepositoryId;
                    branch.Sha = b.Commit.Sha;
                    branch.ApiUrl = b.Commit.Url;
                    lock (thisLock)
                    {
                        _context.GitHubBranches.Add(branch);
                        _context.SaveChanges();
                    }
                }
                await GetBranchFiles(repoData[0], repoData[1], branch);
            }

        }

        private async Task GetBranchFiles(string owner, string name, GitHubBranch branch, string path = "/", GitHubFile parent = null)
        {
            string AccessToken = await HttpContext.GetTokenAsync("access_token");
            var github = new GitHubClient(new ProductHeaderValue("AspNetCoreGitHubAuth"),
                new InMemoryCredentialStore(new Credentials(AccessToken)));

            IReadOnlyList<RepositoryContent> contents = await github.Repository.Content.GetAllContentsByRef(owner, name, path, branch.Name);
            foreach (RepositoryContent c in contents)
            {
                GitHubFile thisFile = new GitHubFile();
                lock (thisLock)
                {
                    thisFile.BranchId = _context.GitHubBranches.Where(b => b.BranchId == branch.BranchId).Single().BranchId;
                }
                thisFile.DownloadPath = c.DownloadUrl;
                thisFile.Name = c.Name;
                thisFile.Path = c.Path;
                if (parent == null)
                {
                    thisFile.ParentId = -1;
                }
                else
                {
                    thisFile.ParentId = parent.FileId;
                }
                if (c.Type == "dir")
                {
                    thisFile.IsDir = true;
                    thisFile.IsFile = false;
                    lock(thisLock)
                    {
                        _context.GitHubFiles.Add(thisFile);
                        _context.SaveChanges();
                    }
                    try
                    {
                        await GetBranchFiles(owner, name, branch, thisFile.Path, thisFile);
                    }
                    catch
                    {

                    }
                }
                else
                {
                    thisFile.IsDir = false;
                    thisFile.IsFile = true;
                    lock (thisLock)
                    {
                        _context.GitHubFiles.Add(thisFile);
                        _context.SaveChanges();
                    }
                }
            }
        }
    }
}