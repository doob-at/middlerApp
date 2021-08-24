using System.Net;

namespace middlerApp.Auth
{
    public static class IdpUriGenerator
    {
        public static string GenerateRedirectUri(string ipAddress, int port)
        {
            var idpListenIp = IPAddress.Parse(ipAddress);
            var isLocalhost = IPAddress.IsLoopback(idpListenIp) || idpListenIp.ToString() == IPAddress.Any.ToString();

            if (isLocalhost)
            {
                return port == 443 ? $"https://localhost" : $"https://localhost:{port}";
            }
            else
            {
                return port == 443
                    ? $"https://{ipAddress}"
                    : $"https://{ipAddress}:{port}";
            }
        }

    }
}
