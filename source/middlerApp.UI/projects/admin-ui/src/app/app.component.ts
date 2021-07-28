import { Component, ChangeDetectionStrategy, OnInit, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { AppInitializeService } from './app-initialize.service';

import { tap, share, map, filter } from 'rxjs/operators';
import { ActivatedRoute, Router, RouterLink, NavigationEnd } from '@angular/router';
import { AppUIService, AuthQuery, AuthService } from './shared/services';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {

    sideBarCollapsed$ = this.uiService.sideBarCollapsed$;
    uiContext$ = this.uiService.UIContext$;
    loggedInUser$ = this.authQuery.loggedInUser$;

    userName$ = this.loggedInUser$.pipe(
        map(user => {
            if (!user) {
                return null;
            }
            var name = user.UserName;
            if(user.FirstName?.trim() && user.LastName?.trim()) {
                name = `${user.FirstName?.trim()} ${user.LastName?.trim()}`
            }
            
            return name;
        })
    );

    constructor(private initService: AppInitializeService, private uiService: AppUIService, private authQuery: AuthQuery, private authService: AuthService) {

        uiService.SetDefault(ui => {
            ui.Content.Scrollable = false;
            ui.Content.Container = true;
            ui.Header.Icon = "";
            ui.Footer.Show = false;
        })

    }
    
       
    identity = false;

    toggleSideBar() {
        this.uiService.toggleSideBar();
    }

    Login() {
        this.authService.LogIn();
    }

    Logout() {
        this.authService.LogOut();
    }

}
