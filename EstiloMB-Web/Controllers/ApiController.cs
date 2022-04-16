using EstiloMB.Core;
using EstiloMB.MVC;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EstiloMB.Site.Controllers
{
    public class ApiController : Controller
    {
        private Localization _loc;
        private ViewRender _viewRender;

        public ApiController(Localization loc, ViewRender viewRender)
        {
            _loc = loc;
            _viewRender = viewRender;
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, Produces("application/json")]
        public IActionResult Ajax([FromRoute] string type, [FromRoute] string function, [FromBody] JObject request)
        {
            return Json(API.Redirect(type, function, User.GetClaimInt32("UsuarioID"), _loc.GetCulture(), request));
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken, Produces("application/json")]
        public IActionResult Page([FromRoute] string control, [FromRoute] string page, [FromBody] JObject request)
        {
            return RedirectToAction(page, control, new { body = true });
        }
    }
}