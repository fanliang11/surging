using Microsoft.AspNetCore.Mvc;
using Surging.ApiGateway.Models;
using System.Diagnostics;

namespace Surging.ApiGateway.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
