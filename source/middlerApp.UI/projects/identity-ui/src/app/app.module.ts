import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AkitaNgRouterStoreModule } from '@datorama/akita-ng-router-store';
import { AkitaNgDevtools } from '@datorama/akita-ngdevtools';
import { NZ_I18N, en_US } from 'ng-zorro-antd/i18n';
import { GlobalImportsModule } from './global-imports.module';
import { environment } from '../environments/environment';

import { AppRoutingModule, RoutingComponents } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthConfigModule } from './auth/auth-config.module';
import { AuthInterceptor } from 'angular-auth-oidc-client';
import { FaConfig, FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { IconsImport } from './icons-import';

@NgModule({
  declarations: [
    AppComponent,
    ...RoutingComponents
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
