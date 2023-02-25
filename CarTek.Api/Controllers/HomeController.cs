using CarTek.Api.DBContext;
using CarTek.Api.Services;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication3.Models;

namespace CarTek.Api
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly ICarService _carService;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext, IUserService userService, ICarService carService)
        {
            _logger = logger;
            _dbContext = dbContext; 
            _carService = carService;
            _userService = userService;
        }

        public IActionResult Index()
        {
            var users = _userService.GetAll();
            return View(users);
        }

        public IActionResult Cars()
        {
            var cars = _carService.GetAll();
            return View(cars);
        }

        public IActionResult Users()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
