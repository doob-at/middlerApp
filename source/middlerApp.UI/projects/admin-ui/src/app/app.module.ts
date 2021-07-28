import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NZ_I18N } from 'ng-zorro-antd/i18n';
import { en_US } from 'ng-zorro-antd/i18n';
import { registerLocaleData } from '@angular/common';
import en from '@angular/common/locales/en';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { IconsProviderModule } from './icons-provider.module';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { AkitaNgDevtools } from '@datorama/akita-ngdevtools';
import { AkitaNgRouterStoreModule } from '@datorama/akita-ng-router-store';
import { environment } from '../environments/environment';
import { NgZorroImportsModule } from './ng-zorro-imports.module';
import { DoobAntdExtensionsModule } from '@doob-ng/antd-extensions';
import { DoobCoreModule } from '@doob-ng/core';
import { FaConfig, FaIconLibrary, FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { OAuthModule, OAuthStorage } from 'angular-oauth2-oidc';
import { AdminUIConfigService } from './admin-ui-config.service';

registerLocaleData(en);

export function storageFactory(): OAuthStorage {
    return localStorage
}

const appInitializerFn = (configService: AdminUIConfigService) => {
    return () => configService.loadConfiguration();
};

@NgModule({
    declarations: [
        AppComponent
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        AppRoutingModule,
        FormsModule,
        HttpClientModule,
        IconsProviderModule,
        NzLayoutModule,
        NzMenuModule,
        environment.production ? [] : AkitaNgDevtools.forRoot(),
        AkitaNgRouterStoreModule,
        NgZorroImportsModule,
        DoobCoreModule,
        DoobAntdExtensionsModule,
        FontAwesomeModule,
        OAuthModule.forRoot(),
    ],
    providers: [
        AdminUIConfigService,
        {
            provide: APP_INITIALIZER,
            useFactory: appInitializerFn,
            multi: true,
            deps: [AdminUIConfigService]
          },
        { provide: NZ_I18N, useValue: en_US },
        { provide: OAuthStorage, useFactory: storageFactory }
    ],
    bootstrap: [AppComponent]
})
export class AppModule {

}
