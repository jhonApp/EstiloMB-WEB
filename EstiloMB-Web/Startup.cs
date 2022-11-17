using EstiloMB.Core;
using EstiloMB.MVC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading.Tasks;

namespace EstiloMB.Site
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("Login")
                    .AddCookie("Login", c => {
                        c.Cookie.Name = "EstiloMB";
                        c.ClaimsIssuer = "EstiloMB";
                        c.LoginPath = new PathString("/Home/Index");
                        c.Events.OnRedirectToLogin = (context) =>
                        {
                            if (context.Request.Headers["Content-Type"] == "application/json")
                            {
                                context.HttpContext.Response.StatusCode = 401;
                            }
                            else
                            {
                                context.Response.Redirect(context.RedirectUri);
                                //context.RedirectUri = c.LoginPath;
                            }

                            return Task.CompletedTask;
                        };
                        c.AccessDeniedPath = new PathString("/Home/Index");
                    });

            services.AddOptions();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ViewRender>();
            services.AddTransient<Localization>();
            services.AddTransient<CrossSiteAntiForgery>();
            services.AddAntiforgery(o => o.HeaderName = CrossSiteAntiForgery.Header);

            services.AddMvc(o =>
            {
                o.Filters.AddService(typeof(CrossSiteAntiForgery));
                o.EnableEndpointRouting = false;
            })
            .AddNewtonsoftJson(o =>
            {
                o.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            //// - System.Text.Json
            //.AddJsonOptions(o =>
            //{
            //    o.JsonSerializerOptions.PropertyNamingPolicy = null;
            //});

#if DEBUG
            if (Environment.EnvironmentName == "Development")
            {
                services.AddRazorPages().AddRazorRuntimeCompilation();
            }
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            //Database.Configure(Configuration.GetConnectionString("Desenvolvimento"));
            Database.Configure(Configuration.GetConnectionString("DesenvolvimentoKstack"));

            Localization.AddDictionary("pt-br", PT_BR.dicionario);
            Localization.AddDictionary("en-us", EN_US.dicionario);

            //Realizar a configuração de cookies
            app.UseAuthentication();

            if (Environment.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "page", template: "Page/{control}/{page}", defaults: new { controller = "Api", action = "Page" });
                routes.MapRoute(name: "api", template: "Api/{type}/{function}", defaults: new { controller = "Api", action = "Ajax" });
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{type?}");
            });
        }
    }
}