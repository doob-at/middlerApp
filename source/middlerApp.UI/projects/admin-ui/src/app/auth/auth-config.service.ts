import { Injectable, InjectionToken } from "@angular/core";
import { LogLevel, OpenIdConfiguration } from "angular-auth-oidc-client";
import { AppConfigService } from "../app-config.service";




@Injectable({ providedIn: 'root' })
export class AuthConfigService {

    constructor(private appConfigService: AppConfigService) {
        
    }

    getConfig(): OpenIdConfiguration {

        let appConf = this.appConfigService.GetConfig();

        return {
            authority: `${appConf.IdpBaseUrl}`,
            redirectUrl: window.location.origin,
            postLogoutRedirectUri: window.location.origin,
            clientId: 'middlerUI',
            scope: 'openid profile email offline_access admin_api',
            responseType: 'id_token token',
            silentRenew: true,
            useRefreshToken: true,
            renewTimeBeforeTokenExpiresInSeconds: 30,
            logLevel: LogLevel.Warn,
            autoUserInfo: true,
            secureRoutes: ["_api/", "/_idp/"]
        };
    }
}
