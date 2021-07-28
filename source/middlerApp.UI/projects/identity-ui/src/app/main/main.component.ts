import { Component } from "@angular/core";
import { AppUIService, AuthQuery, AuthService } from '../shared/services';
import { ActivatedRoute, Router } from '@angular/router';
import { map } from 'rxjs/operators';

@Component({
    templateUrl: './main.component.html',
    styleUrls: ['./main.component.scss']
})
export class MainComponent {

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

    constructor(private uiService: AppUIService, private router: Router, private authQuery: AuthQuery, private authService: AuthService) {

        uiService.SetDefault(ui => {
            ui.Content.Scrollable = false;
            ui.Content.Container = true;
            ui.Header.Icon = ""
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
        this.authService.LogIn();
    }

    Logout() {
        this.authService.LogOut();
    }
    
}