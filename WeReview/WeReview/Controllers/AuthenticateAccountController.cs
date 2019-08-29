using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WeReview.Controllers
{
    public class AuthenticateAccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}