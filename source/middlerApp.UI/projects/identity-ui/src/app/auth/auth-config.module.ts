import { NgModule } from '@angular/core';
import { AuthModule, StsConfigLoader, StsConfigStaticLoader } from 'angular-auth-oidc-client';
import { AuthConfigService } from './auth-config.service';

const authFactory = (configService: AuthConfigService) => {
    const config = configService.getConfig();
    return new StsConfigStaticLoader(config);
};


@NgModule({
    imports: [
        AuthModule.forRoot({
            loader: {
              provide: StsConfigLoader,
              useFactory: authFactory,
              deps: [AuthConfigService],
            },
          }),
    ],
    exports: [AuthModule],
})
export class AuthConfigModule { }
