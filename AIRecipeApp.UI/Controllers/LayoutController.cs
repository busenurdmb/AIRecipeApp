using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIRecipeApp.UI.Controllers
{
    [AllowAnonymous]
    public class LayoutController : Controller
    {
        public IActionResult _Layout()
        {
            return View();
        }
    }
}
