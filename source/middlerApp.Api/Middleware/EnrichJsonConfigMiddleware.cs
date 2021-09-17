using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using doob.middler.Common.StreamHelper;
using doob.Reflectensions;
using doob.Reflectensions.ExtensionMethods;
using doob.SignalARRR.Server.ExtensionMethods;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using middlerApp.Api.ExtensionMethods;
using Newtonsoft.Json.Linq;

namespace middlerApp.Api.Middleware
{
    public class EnrichJsonConfigMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly StartUpConfiguration _configuration;


        public EnrichJsonConfigMiddleware(RequestDelegate next, StartUpConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {


            if (context.Request.Method.ToUpper() != "GET")
            {
                await _next(context);
                return;
            }

            var appConfigPath = $"/{context.Request.Path.Value?.TrimStart('/')}";

            //_configuration.EnrichJson
            //var enrichSection = _configuration.GetSection("EnrichJson");
            //var childs = enrichSection.GetChildren();
            var enrich = _configuration.EnrichJson.FirstOrDefault(c =>
                c.Key.StartsWith("path_") && $"/{c.Value?.ToString().TrimStart('/')}" == appConfigPath);

            //var enrich = Static.EnvVariables.FirstOrDefault(env => env.Key.StartsWith("enrich_") && $"/{env.Value?.ToString().TrimStart('/')}" == appConfigPath);

            if (enrich.Key == null)
            {
                await _next(context);
                return;
            }

            var origStream = context.Response.Body;
            context.Response.Body = new MemoryStream();

            await _next(context);


            var body = context.Response.Body;
            body.Seek(0, SeekOrigin.Begin);

            var sr = new StreamReader(body);
            var str = await sr.ReadToEndAsync();

            body.Seek(0, SeekOrigin.Begin);

            IDictionary<string, object> config;
            try
            {
                config = Json.Converter.FromJsonStreamToObject<Dictionary<string, object>>(body);
            }
            catch
            {
                return;
            }


            if (config == null)
            {
                return;
            }

            context.Response.Body = origStream;

            var prefix = enrich.Key.Split('_')[1];
            //var appConfigFilePath = enrich.Value.ToString();

            //if (context.IsAdminAreaRequest())
            //{

            //}
            //FileInfo configFile = new FileInfo(Path.Combine(_hostingEnvironment.WebRootPath, appConfigFilePath));

            //IDictionary<string, object> config = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);


            //if (configFile.Exists)
            //{
            //    var configJson = await System.IO.File.ReadAllTextAsync(configFile.FullName);
            //    try
            //    {
            //        var dict = Json.Converter.ToDictionary(configJson);
            //        config = new Dictionary<string, object>(dict, StringComparer.CurrentCultureIgnoreCase);
            //    }
            //    catch
            //    {
            //        // ignored
            //    }
            //}


            foreach (var kvp in _configuration.EnrichJson)
            {
                if (kvp.Key.StartsWith($"{prefix}_"))
                {
                    var key = kvp.Key.Substring($"{prefix}_".Length);
                    config[key] = kvp.Value;
                }

            }

            var appConfig = Json.Converter.Unflatten(config) ?? new JObject();

            var res = new ObjectResult(appConfig);
            res.ContentTypes.Add("application/json");

            await context.WriteActionResult(res); //.Response.WriteAsync(Json.Converter.ToJson(appConfig));


        }


    }

    public static class EnrichJsonConfigMiddlewareExtensions
    {
        public static IApplicationBuilder UseEnrichAppConfigMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EnrichJsonConfigMiddleware>();
        }

    }

}

