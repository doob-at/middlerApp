using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using middlerApp.Api.Helper;

namespace middlerApp.Api.ExtensionMethods
{
    public static class SpaExtensions
    {

        public static void UseSpaUI(this IApplicationBuilder app)
        {

            var wenv = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            UseSpaUI(app, wenv.WebRootPath);
        }
        public static void UseSpaUI(this IApplicationBuilder app, string path)
        {

                var stfOptions = new StaticFileOptions()
                {
                    RequestPath = "",
                    FileProvider = new PhysicalFileProvider(PathHelper.GetFullPath(path)),
                    OnPrepareResponse = ctx =>
                    {
                        if (ctx.Context.Request.Path.ToString() == "/index.html")
                        {
                            var headers = ctx.Context.Response.GetTypedHeaders();
                            headers.CacheControl = new CacheControlHeaderValue
                            {
                                Public = true,
                                MaxAge = TimeSpan.FromDays(0)
                            };
                        }
                    }
                };

                

                app.UseDefaultFiles();
                app.UseStaticFiles(stfOptions);

                app.UseSpa(spa =>
                {
                    spa.Options.DefaultPageStaticFileOptions = stfOptions;
                    
                    
                });
            
        }

        public static void UseSpaUI(this IApplicationBuilder app, string path, string devProxy)
        {

            if (Static.IsDevelopment)
            {
                app.UseSpa(spa =>
                {
                    spa.UseProxyToSpaDevelopmentServer(devProxy);
                });
            }
            else
            {

                var stfOptions = new StaticFileOptions()
                {
                    RequestPath = "",
                    FileProvider = new PhysicalFileProvider(PathHelper.GetFullPath(path)),
                    OnPrepareResponse = ctx =>
                    {
                        if (ctx.Context.Request.Path.ToString() == "/index.html")
                        {
                            var headers = ctx.Context.Response.GetTypedHeaders();
                            headers.CacheControl = new CacheControlHeaderValue
                            {
                                Public = true,
                                MaxAge = TimeSpan.FromDays(0)
                            };
                        }
                    }
                };


                app.UseDefaultFiles();
                app.UseStaticFiles(stfOptions);

                app.UseSpa(spa =>
                {
                    spa.Options.DefaultPageStaticFileOptions = stfOptions;
                });
            }

            
        }
    }
}
