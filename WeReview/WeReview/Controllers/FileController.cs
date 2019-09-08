using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeReview.Data;
using WeReview.Models;
using Octokit;
using Microsoft.AspNetCore.Authentication;
using Octokit.Internal;
using System.Security.Claims;

namespace WeReview.Controllers
{
    [Route("[controller]/[action]")]
    public class FileController : Controller
    {
        private readonly object thisLock;
        private ApplicationDbContext _context;

        public FileController(ApplicationDbContext context)
        {
            _context = context;
            thisLock = new object();
        }
        public async Task<IActionResult> Index(int fileId)
        {
            List<GitHubLine> selectedLines = new List<GitHubLine>();
            string AccessToken = await HttpContext.GetTokenAsync("access_token");
            var github = new GitHubClient(new ProductHeaderValue("AspNetCoreGitHubAuth"),
               new InMemoryCredentialStore(new Credentials(AccessToken)));
            GitHubFile matchingFile;
            GitHubBranch fileBranch;
            GitHubRepository branchRepo;
            lock (thisLock)
            {
                matchingFile = _context.GitHubFiles.Where(f => f.FileId == fileId).SingleOrDefault();
                fileBranch = _context.GitHubBranches.Where(b => matchingFile.BranchId == b.BranchId).SingleOrDefault();
                branchRepo = _context.GitHubRepos.Where(r => r.RepositoryId == fileBranch.RepositoryId).SingleOrDefault();
            }
            string[] repoData = branchRepo.FullName.Split('/');

            IReadOnlyList<RepositoryContent> fileContents = await github.Repository.Content.GetAllContentsByRef(repoData[0], repoData[1], matchingFile.Path, fileBranch.Name);
            if (fileContents[0].Name[fileContents[0].Name.Length - 1] == 's' && fileContents[0].Name[fileContents[0].Name.Length - 2] == 'c' && fileContents[0].Name[fileContents[0].Name.Length - 3] == '.')
            {
                string fileContentsText = fileContents[0].Content;
                string[] fileText = fileContentsText.Split('\n');
                for (int i = 1; i <= fileText.Length; i++)
                {
                    GitHubLine thisLine;
                    lock (thisLock)
                    {
                        thisLine = _context.GitHubLines.Where(l => l.LineInFile == i).Where(l => l.FileId == matchingFile.FileId).SingleOrDefault();
                    }
                    if (thisLine == null)
                    {
                        thisLine = new GitHubLine();
                        thisLine.FileId = matchingFile.FileId;
                        thisLine.Content = fileText[i - 1];
                        thisLine.LineInFile = i;
                        thisLine.IsApproved = false;
                        thisLine.IsChanged = false;
                        thisLine.IsReviewed = false;
                        thisLine.Value = 1;
                        lock (thisLock)
                        {
                            _context.GitHubLines.Add(thisLine);
                        }
                        matchingFile.FileValue++;
                    }

                    selectedLines.Add(thisLine);
                }
            }
            int unchanged = 0;
            int unapproved = 0;
            int approved = 0;
            int rejected = 0;
            foreach (GitHubLine line in selectedLines)
            {
                if (!line.IsChanged)
                {
                    unchanged++;
                }
                else if (!line.IsReviewed)
                {
                    unapproved++;
                }
                else if (!line.IsApproved)
                {
                    rejected++;
                }
                else
                {
                    approved++;
                }
            }
            matchingFile.ApprovedValue = approved;
            matchingFile.RejectedValue = rejected;
            matchingFile.UnapprovedValue = unapproved;
            matchingFile.UnchangedValue = unchanged;
            matchingFile.FileValue = approved + rejected + unapproved + unchanged;
            lock(thisLock)
            {
                _context.SaveChanges();
            }
            ViewData["File"] = matchingFile;
            return View(selectedLines);
        }

        [HttpPost]
        public void Index([FromBody]GitHubReview review)
        {
            List<GitHubLine> linesForFile;
            GitHubUser reviewUser;
            string githubId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            lock (thisLock)
            {
                linesForFile = _context.GitHubLines.Where(l => l.FileId == review.FileId).ToList();
                reviewUser = _context.GitHubUsers.Where(u => u.GitHubUserId == githubId).Single();
                review.User = reviewUser;
                review.UserId = review.User.UserId;
            }
            
            List<GitHubLine> linesMatching = new List<GitHubLine>();

            foreach(int i in review.LineIds)
            {
                GitHubLine thisLine = linesForFile.Where(l => l.LineInFile == i).Single();
                thisLine.IsApproved = review.IsApproved;
                thisLine.IsReviewed = true;
                linesMatching.Add(thisLine);
            }
            lock (thisLock)
            {
                review.File = _context.GitHubFiles.Where(f => f.FileId == review.FileId).Single();
                _context.GitHubReviews.Add(review);
                _context.SaveChanges();
            }            
        }
    }
}