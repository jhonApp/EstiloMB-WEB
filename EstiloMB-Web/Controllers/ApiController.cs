using Chargeback.Core;
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

        [HttpPost, Produces("application/json")]
        public async Task<IActionResult> Login([FromBody] Request<Usuario> request)
        {
            //Response<Usuario> response = Usuario.Login(usuario.Email, usuario.Senha, usuario.NovaSenha, Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString());
            Response<Usuario> response = Usuario.Login(request);

            if (response.Code != ResponseCode.Sucess)
            {
                Response<JObject> error = new Response<JObject>();
                error.Data = response.Validation?.ToJObject();
                error.Code = response.Code;
                error.Message = response.Message;

                return Json(error);
            }

            ClaimsIdentity identity = new ClaimsIdentity("Login");
            identity.AddClaim(new Claim("UsuarioID", response.Data.UsuarioID.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, response.Data.Nome));

            List<Claim> claims = new List<Claim>();

            for (int i = 0; i < response.Data.Perfis.Count; i++)
            {
                for (int j = 0; j < response.Data.Perfis[i].Perfil.Acoes.Count; j++)
                {
                    if (!response.Data.Perfis[i].Perfil.Acoes[j].Habilitado) { continue; }
                    if (claims.Any(e => e.Type == response.Data.Perfis[i].Perfil.Acoes[j].Nome)) { continue; }

                    claims.Add(new Claim(response.Data.Perfis[i].Perfil.Acoes[j].Nome, "true"));
                }
            }

            for (int i = 0; i < claims.Count; i++)
            {
                identity.AddClaim(claims[i]);
            }

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            try
            {
                await HttpContext.SignInAsync("Login", principal,
                    new AuthenticationProperties
                    {
                        ExpiresUtc = request.Data.LembrarMe ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddHours(8),
                        AllowRefresh = true,
                        IsPersistent = true
                    });
            }
            catch (Exception ex)
            {
                Response error = new Response();
                error.Code = ResponseCode.ServerError;
                error.Message = ex.Message;

                return Json(error);
            }

            return Json(response);
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