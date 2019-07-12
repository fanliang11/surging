using Microsoft.AspNetCore.Mvc;
using Surging.ApiGateway.Models;
using System.Diagnostics;

namespace Surging.ApiGateway.Controllers
{
    /// <summary>
    /// Defines the <see cref="HomeController" />
    /// </summary>
    public class HomeController : Controller
    {
        #region 方法

        /// <summary>
        /// The Error
        /// </summary>
        /// <returns>The <see cref="IActionResult"/></returns>
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// The Index
        /// </summary>
        /// <returns>The <see cref="IActionResult"/></returns>
        public IActionResult Index()
        {
            return View();
        }

        #endregion 方法
    }
}