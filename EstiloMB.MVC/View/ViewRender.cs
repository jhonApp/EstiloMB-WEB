using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EstiloMB.MVC
{
    /* How to set it up in [Startup.cs]:
     * 
     * public void ConfigureServices(IServiceCollection services)
     * {
     *      // - Register the service for use within the application.
     *      services.AddScoped<ViewRender>();
     * }
     * 
     * How to use it in your controller:
     * 
     * public class HomeController
     * {
     *      private ViewRender viewRender;
     *      // - Requests the registered service via injection in the constructor.
     *      public HomeController(ViewRender _viewRender)
     *      {
     *          viewRender = _viewRender;
     *      }
     *      
     *      // - The action has to be async.
     *      public async Task<IActionResult> Index()
     *      {
     *          string viewContentString = await viewRender.RenderView("MyViewName", myModel);
     *          return View();
     *      }
     * }
     */
    /// <summary>
    /// 
    /// </summary>
    public class ViewRender
    {
        private IRazorViewEngine _viewEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITempDataProvider _tempDataProvider;

        public ViewRender(IRazorViewEngine viewEngine, IServiceProvider serviceProvider, ITempDataProvider tempDataProvider)
        {
            _viewEngine = viewEngine;
            _serviceProvider = serviceProvider;
            _tempDataProvider = tempDataProvider;
        }

        public async Task<string> RenderViewAsync(string viewName)
        {
            return await RenderViewAsync(viewName, null);
        }

        public async Task<string> RenderViewAsync(string viewName, object model)
        {
            DefaultHttpContext httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };

            ActionContext actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using (StringWriter outputWriter = new StringWriter())
            {
                ViewEngineResult viewResult = _viewEngine.FindView(actionContext, viewName, false);
                ViewDataDictionary viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };
                TempDataDictionary tempDataDictionary = new TempDataDictionary(httpContext, _tempDataProvider);

                if (!viewResult.Success)
                {
                    throw new FileNotFoundException($"The view {viewName} was not found.");
                }

                try
                {
                    ViewContext viewContext = new ViewContext(actionContext, viewResult.View, viewDictionary, tempDataDictionary, outputWriter, new HtmlHelperOptions());
                    await viewResult.View.RenderAsync(viewContext);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"The Razor Engine was unable to render {viewName}.", ex);
                }

                return outputWriter.ToString();
            }
        }
    }
}
