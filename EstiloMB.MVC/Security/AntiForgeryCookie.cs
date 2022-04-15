using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EstiloMB.MVC
{
    /* How to set it up in [Startup.cs]:
     * 
     * public void ConfigureServices(IServiceCollection services)
     * {
     *      // - Enable options.
     *      services.AddOptions();
     *      
     *      // - Register the cross forgery service for use within the application, it's required since it'll be a MVC filter.
     *      services.AddTransient<CrossSiteAntiForgery>();
     *      
     *      // - Set the header name of the antiforgery service.
     *      services.AddAntiforgery(options => options.HeaderName = CrossSiteAntiForgery.Header);
     *      
     *      // - Add it as a MVC filter.
     *      services.AddMvc(options => 
     *      {
     *          options.Filters.AddService(typeof(CrossSiteAntiForgery));
     *      });
     * }
     * 
     * Use it as usual in your actions:
     * 
     * [ValidateAntiForgeryToken]
     * public IActionResult Index()
     * {
     *      return View();
     * }
     */
    /// <summary>
    /// 
    /// </summary>
    public class CrossSiteAntiForgery : ResultFilterAttribute
    {
        public const string Cookie = "XSRF-TOKEN";
        public const string Header = "X-XSRF-TOKEN";

        private IAntiforgery _antiforgery;
        public CrossSiteAntiForgery(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            //if (context.Result is ViewResult)
            //{
                //string contentType = (context.Result as ViewResult).ContentType;
                AntiforgeryTokenSet tokens = _antiforgery.GetAndStoreTokens(context.HttpContext);
                context.HttpContext.Response.Cookies.Append(Cookie, tokens.RequestToken, new CookieOptions() { HttpOnly = false });
            //}
        }

        // - Before services.AddMvc()
        //public static void Startup(IServiceCollection services)
        //{
        //    services.AddAntiforgery(o => o.HeaderName = "X-XSRF-TOKEN");
        //    services.AddTransient<XSRFCookie>();
        //}
    }
}