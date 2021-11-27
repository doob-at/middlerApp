import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NZ_I18N } from 'ng-zorro-antd/i18n';
import { en_US } from 'ng-zorro-antd/i18n';
import { registerLocaleData } from '@angular/common';
import en from '@angular/common/locales/en';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AkitaNgDevtools } from '@datorama/akita-ngdevtools';
import { AkitaNgRouterStoreModule } from '@datorama/akita-ng-router-store';
import { environment } from '../environments/environment';
import { AuthConfigModule } from './auth/auth-config.module';
import { GlobalImportsModule } from './global-imports.module';
import { FaConfig, FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { IconsImport } from './icons-import';
import { AuthInterceptor } from 'angular-auth-oidc-client';
// import { AppConfigService } from './app-config.service';

registerLocaleData(en);

// const appInitializerFn = (appConfig: AppConfigService) => {
//   return () => {
//     return appConfig.loadAppConfig();
//   };
// };


@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ReactiveFormsModule,
    HttpClientModule,
    BrowserAnimationsModule,
    environment.production ? [] : AkitaNgDevtools.forRoot(),
    AkitaNgRouterStoreModule,
    AuthConfigModule,
    GlobalImportsModule
  ],
  providers: [
    // AppConfigService,
    // {
    //   provide: APP_INITIALIZER,
    //   useFactory: appInitializerFn,
    //   multi: true,
    //   deps: [AppConfigService]
    // },
    {
      provide: NZ_I18N,
      useValue: en_US
    },
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {

  constructor(private library: FaIconLibrary, faConfig: FaConfig) {

    var fLib = new IconsImport();
    fLib.Init(library);

  }

}
