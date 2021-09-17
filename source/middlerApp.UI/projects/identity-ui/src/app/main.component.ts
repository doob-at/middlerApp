import { Component } from "@angular/core";
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { map } from 'rxjs/operators';
import { AppUIService } from '@shared/services';
import { AuthQuery } from '@shared/auth/auth.store';
import { AuthService } from '@shared/auth/auth.service';

@Component({
    templateUrl: "main.component.html",
    styleUrls: ['./main.component.scss']
})
export class MainComponent {

    sideBarCollapsed$ = this.uiService.sideBarCollapsed$;
    uiContext$ = this.uiService.UIContext$;
    loggedInUser$ = this.authQuery.loggedInUser$;
    UIMenu = true;

    userName$ = this.loggedInUser$.pipe(
        map(user => {
            if (!user) {
                return null;
            }
            var name = user.UserName;
            if (user.FirstName?.trim() && user.LastName?.trim()) {
                name = `${user.FirstName?.trim()} ${user.LastName?.trim()}`
            }

            return name;
        })
    );

    constructor(
        //private initService: AppInitializeService, 
        private uiService: AppUIService,
        private authQuery: AuthQuery,
        private authService: AuthService, 
        public oidcSecurityService: OidcSecurityService) {

        uiService.Set(ui => {
            ui.Header.Title = "Administration"
            ui.Header.Icon = "setting";
        })

    }

    Login() {
        this.oidcSecurityService.authorize();
    }

    Logout() {
        this.oidcSecurityService.logoffAndRevokeTokens().subscribe(ev => console.log(ev))
    }
    
}