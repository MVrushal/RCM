using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Integr8ed.Areas.User.Controllers
{
    [Area("User")]
    public class RoomAvailabilityController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}