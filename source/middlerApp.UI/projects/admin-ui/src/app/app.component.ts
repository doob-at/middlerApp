import { Component, ChangeDetectionStrategy, OnInit, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { AppInitializeService } from './app-initialize.service';
import { AppUIService, AuthService } from '@admin-services';
import { tap, share, map, filter } from 'rxjs/operators';
import { ActivatedRoute, Router, RouterLink, NavigationEnd } from '@angular/router';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements AfterViewInit {

    uiContext$ = this.uiService.UIContext$;
    sideBarCollapsed$ = this.uiService.sideBarCollapsed$;

    currentUser$ = this.auth.currentUser$;

    userName$ = this.currentUser$.pipe(
        map(user => {
            if (!user) {
                return null;
            }
            var name = user.userName;
            if(user.firstName?.trim() && user.lastName?.trim()) {
                name = `${user.firstName?.trim()} ${user.lastName?.trim()}`
            }
            

            return name;
        })
    );

    constructor(private uiService: AppUIService, private router: Router, private auth: AuthService, private route: ActivatedRoute, private cref: ChangeDetectorRef) {

        uiService.SetDefault(ui => {
            ui.Content.Scrollable = false;
            ui.Content.Container = true;
            ui.Header.Icon = null
            ui.Footer.Show = false;
        })

        
               
        
    }
    ngAfterViewInit(): void {
        
        // if(location.pathname == "/first-setup") {
        //     setTimeout(() => {
        //         this.uiService.Set(ui => {
        //             ui.Sidebar.Hide = true;
        //             ui.Content.ShowAlways = true;
        //             this.cref.detectChanges();
        //         })
        //     }, 1000);
            
        // }
    }
    
    identity = false;

    toggleSideBar() {
        this.uiService.toggleSideBar();
    }

    Login() {
        this.auth.LogIn();
    }

    Logout() {
        this.auth.LogOut();
    }

}
