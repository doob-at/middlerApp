const PROXY_CONFIG = [
    {
        context: [
            "/_NICHTS"
        ],
        target: "https://localhost:4445",
        changeOrigin: true,
        secure: false,
        "bypass": function (req, res, proxyOptions) {
            
            req.headers["X-Forwarded-For"] = "127.0.0.1";
            req.headers["X-Forwarded-Proto"] = "http"
            req.headers["X-Forwarded-Host"] = "http://localhost:4300"
        },
        onProxyRes: proxyRes => {
            let key = 'www-authenticate';
            proxyRes.headers[key] = proxyRes.headers[key] && proxyRes.headers[key].split(',');
        }
    }
]

module.exports = PROXY_CONFIG;