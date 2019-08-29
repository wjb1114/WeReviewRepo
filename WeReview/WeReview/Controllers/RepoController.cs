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

            GetBranchData(repo.ApiUrl);

            return View(repo);
        }

        private void GetBranchData(string apiPath)
        {
            string test = User.Identity.AuthenticationType;
            test = User.Identity.Name;
            string data = string.Empty;
            string url = apiPath + @"/branches";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                data = reader.ReadToEnd();
            }

            JObject returnData = JObject.Parse(data);

            for (int i = 0; i < returnData.Count; i++)
            {
                JToken branchObject = returnData[i];
            }

            
        }
    }
}