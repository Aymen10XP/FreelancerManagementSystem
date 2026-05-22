using Microsoft.AspNetCore.Mvc;

namespace FreelancerManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login() => View();
        public IActionResult Register() => View();
    }
}
