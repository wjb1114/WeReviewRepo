using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Octokit;
using Octokit.Internal;
using WeReview.Data;
using WeReview.Models;

namespace WeReview.Pages
{
    public class IndexModel : PageModel
    {
        public string GitHubAvatar { get; set; }

        public string GitHubLogin { get; set; }

        public string GitHubName { get; set; }

        public string GitHubUrl { get; set; }

        public GitHubUser ThisUser { get; set; }

        public IReadOnlyList<Repository> Repositories { get; set; }

        public List<GitHubRepository> GitHubRepos { get; set; }

        private ApplicationDbContext _context;

        private readonly object thisLock;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
            thisLock = new object();
            GitHubRepos = new List<GitHubRepository>();
        }

        public async Task OnGetAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                GitHubName = User.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;
                GitHubLogin = User.FindFirst(c => c.Type == "urn:github:login")?.Value;
                GitHubUrl = User.FindFirst(c => c.Type == "urn:github:url")?.Value;
                GitHubAvatar = User.FindFirst(c => c.Type == "urn:github:avatar")?.Value;

                GitHubUser foundUser;

                lock(thisLock)
                {
                    foundUser = _context.GitHubUsers.Where(u => u.Username == GitHubLogin).FirstOrDefault();
                }

                if (foundUser == null)
                {
                    foundUser = new GitHubUser();
                    foundUser.Username = GitHubLogin;
                    foundUser.GitHubUrl = GitHubUrl;
                    lock(thisLock)
                    {
                        _context.GitHubUsers.Add(foundUser);
                        _context.SaveChanges();
                    }
                }

                ThisUser = foundUser;

                string AccessToken = await HttpContext.GetTokenAsync("access_token");

                var github = new GitHubClient(new ProductHeaderValue("AspNetCoreGitHubAuth"),
                    new InMemoryCredentialStore(new Credentials(AccessToken)));
                Repositories = await github.Repository.GetAllForCurrent();

                foreach (Repository repo in Repositories)
                {
                    GitHubRepository matchingRepo;
                    GitHubUserRepository matchingUserRepo;
                    lock(thisLock)
                    {
                        matchingRepo = _context.GitHubRepos.Where(r => repo.Archived == false).Where(r => repo.FullName == r.FullName).SingleOrDefault();
                        
                    }
                    if (matchingRepo == null)
                    {
                        matchingRepo = new GitHubRepository();
                        GitHubUserRepository userRepo = new GitHubUserRepository();
                        matchingRepo.FullName = repo.FullName;
                        matchingRepo.CloneUrl = repo.CloneUrl;
                        matchingRepo.ApiUrl = repo.Url;
                        lock (thisLock)
                        {
                            _context.GitHubRepos.Add(matchingRepo);
                            GitHubRepos.Add(matchingRepo);
                            _context.SaveChanges();
                        }
                        userRepo.RepositoryId = matchingRepo.RepositoryId;
                        userRepo.UserId = ThisUser.UserId;
                        lock (thisLock)
                        {
                            userRepo.User = _context.GitHubUsers.Where(u => u.UserId == userRepo.UserId).Single();
                            userRepo.Repository = _context.GitHubRepos.Where(r => r.RepositoryId == userRepo.RepositoryId).Single();
                            _context.GitHubUserRepos.Add(userRepo);
                            _context.SaveChanges();
                        }
                    }
                    else
                    {
                        lock (thisLock)
                        {
                            matchingRepo.UserRepositories = _context.GitHubUserRepos.Where(u => u.RepositoryId == matchingRepo.RepositoryId).ToList();
                        }
                        GitHubRepos.Add(matchingRepo);
                        lock (thisLock)
                        {
                            matchingUserRepo = _context.GitHubUserRepos.Where(u => u.RepositoryId == matchingRepo.RepositoryId).Where(u => u.UserId == ThisUser.UserId).SingleOrDefault();
                        }
                        if (matchingUserRepo == null)
                        {
                            GitHubUserRepository userRepo = new GitHubUserRepository();
                            userRepo.RepositoryId = matchingRepo.RepositoryId;
                            userRepo.UserId = ThisUser.UserId;
                            lock (thisLock)
                            {
                                userRepo.User = _context.GitHubUsers.Where(u => u.UserId == userRepo.UserId).Single();
                                userRepo.Repository = _context.GitHubRepos.Where(r => r.RepositoryId == userRepo.RepositoryId).Single();
                                _context.GitHubUserRepos.Add(userRepo);
                                _context.SaveChanges();
                            }
                        }
                    }
                }
            }
        }
    }
}
