using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace middlerApp.Api.ExtensionMethods
{
    public static class HttpRequestExtensions
    {

        public static List<IPAddress> FindSourceIp(this HttpRequest httpRequest)
        {


            var sourceIps = new List<IPAddress>();


            if (httpRequest.Headers.ContainsKey("X-Forwarded-For"))
            {
                var ips = httpRequest.Headers["X-Forwarded-For"];
                sourceIps = ips.Select(IPAddress.Parse).ToList();
            }

            sourceIps.Add(httpRequest.HttpContext.Connection.RemoteIpAddress);


            return sourceIps;
        }

        /// <summary>
        /// Retrieve the raw body as a string from the Request.Body stream
        /// </summary>
        /// <param name="request">Request instance to apply to</param>
        /// <param name="encoding">Optional - Encoding, defaults to UTF8</param>
        /// <returns></returns>
        public static async Task<string> GetRawBodyStringAsync(this HttpRequest request, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            using (StreamReader reader = new StreamReader(request.Body, encoding))
                return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Retrieves the raw body as a byte array from the Request.Body stream
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<byte[]> GetRawBodyBytesAsync(this HttpRequest request)
        {
            using (var ms = new MemoryStream(2048))
            {
                await request.Body.CopyToAsync(ms);
                return ms.ToArray();
            }
        }

    }
}


