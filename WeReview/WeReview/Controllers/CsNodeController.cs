using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeReview.Data;
using WeReview.Models;

namespace WeReview.Controllers
{
    [Route("[controller]/[action]")]
    public class CsNodeController : Controller
    {
        private readonly object thisLock;
        private ApplicationDbContext _context;

        public CsNodeController(ApplicationDbContext context)
        {
            _context = context;
            thisLock = new object();
        }
        [HttpPost]
        public IActionResult Index(int fileId)
        {
            GitHubFile thisFile;
            lock (thisLock)
            {
                thisFile = _context.GitHubFiles.Where(f => f.FileId == fileId).Single();
            }
            BuildNodeTree(thisFile);
            return View();
        }

        private void BuildNodeTree(GitHubFile file, int? nodeId = null)
        {
            int currentBracketCount = 0;
            bool childNodeGenerationStarted = false;
            CsNode thisNode = new CsNode();
            if (nodeId == null)
            {
                thisNode.ParentNode = null;
                thisNode.ParentNodeId = null;
                thisNode.FileId = file.FileId;
                thisNode.File = file;
                thisNode.NodeName = "File";
                thisNode.LineIds = new List<int>();
                thisNode.ChildNodeIds = new List<int>();
                List<GitHubLine> linesInFile;
                lock(thisLock)
                {
                    linesInFile = _context.GitHubLines.Where(l => l.FileId == file.FileId).ToList();
                }
                foreach (GitHubLine l in linesInFile)
                {
                    thisNode.LineIds.Add(l.LineInFile);
                }
                lock (thisLock)
                {
                    _context.CsNodes.Add(thisNode);
                    _context.SaveChanges();
                }
            }
            else
            {
                currentBracketCount = -1;
                lock (thisLock)
                {
                    thisNode = _context.CsNodes.Where(n => n.Id == nodeId).Single();
                }
            }
            
            List<GitHubLine> fileLines;
            lock (thisLock)
            {
                fileLines = _context.GitHubLines.Where(l => l.FileId == file.FileId).OrderBy(l => l.LineInFile).ToList();
            }
            CsNode childNode = new CsNode
            {
                NodeName = "",
                ParentNodeId = thisNode.Id,
                ParentNode = thisNode,
                LineIds = new List<int>(),
                FileId = file.FileId,
                File = file,
                ChildNodeIds = new List<int>(),
            };
            int firstLine = thisNode.LineIds.Min(i => i);
            int lastLine = thisNode.LineIds.Max(i => i);
            for (int i = firstLine - 1; i < lastLine; i++)
            {
                string thisLine = fileLines[i].Content;
                string nextLine = null;
                if (i + 1 < fileLines.Count)
                {
                    nextLine = fileLines[i + 1].Content;
                }
                else
                {
                    nextLine = "";
                }
                if((thisLine.Contains("namespace") || thisLine.Contains("class") || (thisLine.Contains('(') && thisLine.Contains(')') && !thisLine.Contains(';'))) && currentBracketCount == 0 && childNodeGenerationStarted == false)
                {
                    childNode = new CsNode();
                    childNodeGenerationStarted = true;
                    if (thisLine.Contains("namespace"))
                    {
                        childNode.NodeName = "Namespace";
                    }
                    else if(thisLine.Contains("class"))
                    {
                        childNode.NodeName = "Class";
                    }
                    else if (thisLine.Contains('(') && thisLine.Contains(')') && !thisLine.Contains(';'))
                    {
                        childNode.NodeName = "Method";
                    }
                    else
                    {
                        childNode.NodeName = "Error";
                    }
                    childNode.ParentNodeId = thisNode.Id;
                    childNode.ParentNode = thisNode;
                    childNode.LineIds = new List<int>();
                    childNode.FileId = file.FileId;
                    childNode.File = file;
                    childNode.ChildNodeIds = new List<int>();
                    childNode.LineIds.Add(fileLines[i].LineInFile);
                    if(thisLine.Contains('{'))
                    {
                        currentBracketCount++;
                    }
                }
                else
                {
                    if (childNodeGenerationStarted == true)
                    {
                        childNode.LineIds.Add(fileLines[i].LineInFile);
                    }
                    if(thisLine.Contains('{'))
                    {
                        currentBracketCount++;
                    }
                    else if (thisLine.Contains('}'))
                    {
                        currentBracketCount--;
                    }

                    if(currentBracketCount == 0 && childNodeGenerationStarted == true)
                    {
                        childNodeGenerationStarted = false;
                        lock(thisLock)
                        {
                            _context.CsNodes.Add(childNode);
                            _context.SaveChanges();
                            thisNode.ChildNodeIds.Add(childNode.Id);
                            _context.SaveChanges();
                        }
                    }
                }
            }
            foreach(int childNodeId in thisNode.ChildNodeIds)
            {
                CsNode thisChildNode = _context.CsNodes.Where(n => n.Id == childNodeId).Single();
                BuildNodeTree(file, thisChildNode.Id);
            }
        }
    }
}