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

        private ApplicationDbContext _context;

        private readonly object thisLock;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
            thisLock = new object();
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
            }
        }
    }
}
