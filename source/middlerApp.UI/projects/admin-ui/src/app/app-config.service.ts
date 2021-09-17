import { Inject, Injectable, InjectionToken } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class AppConfigService {

    constructor(@Inject(APP_CONFIG)private appConfig: AppConfig) {
        
    }

    public GetConfig() {
        return this.appConfig;
    }
}

export const APP_CONFIG = new InjectionToken<AppConfig>('app.config');

export class AppConfig {
    Debug: boolean = true;
    IdpBaseUrl: string = "";
}