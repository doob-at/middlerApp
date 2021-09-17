import { Injectable, InjectionToken } from "@angular/core";
import { LogLevel, OpenIdConfiguration } from "angular-auth-oidc-client";


@Injectable({ providedIn: 'root' })
export class AuthConfigService {

    getConfig(): OpenIdConfiguration {
        
        return {
            authority: window.location.origin,
            redirectUrl: window.location.origin,
            postLogoutRedirectUri: window.location.origin,
            clientId: 'identityUI',
            scope: 'openid profile email offline_access idp_api',
            responseType: 'code',
            silentRenew: true,
            useRefreshToken: true,
            renewTimeBeforeTokenExpiresInSeconds: 30,
            logLevel: LogLevel.Warn,
            autoUserInfo: true,
            secureRoutes: ["_api/", "/_api/"],
        };
    }
}

