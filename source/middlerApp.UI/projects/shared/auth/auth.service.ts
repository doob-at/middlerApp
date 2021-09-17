import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { LoggedInUser } from "@shared/models/logged-in-user";
import { EventTypes, OidcSecurityService, PublicEventsService } from "angular-auth-oidc-client";
import { filter } from "rxjs/operators";
import { AuthStore } from "./auth.store";


@Injectable({
    providedIn: 'root'
})
export class AuthService {

    constructor(
        private http: HttpClient,
        private router: Router,
        public oidcSecurityService: OidcSecurityService,
        private eventService: PublicEventsService,
        private idpStore: AuthStore) {

        this.oidcSecurityService.checkAuth().subscribe((ev) => {
            // let lUser = new LoggedInUser();
            // if (ev.isAuthenticated) {
            //     lUser.Roles = ev.userData.roles;
            //     lUser.UserName = ev.userData.name;
            //     lUser.IsAdmin = lUser.Roles?.indexOf("Administrators") != -1;
            // }
            // this.idpStore.update(state => ({
            //     LoggedInUser: lUser
            // }));
        });


        this.eventService
            .registerForEvents()
            .pipe(
                filter((notification) => notification.type === EventTypes.UserDataChanged)
            )
            .subscribe((value) => {
                let lUser = new LoggedInUser();
                if (value?.value?.userData) {
                    lUser.Roles = value.value.userData.roles;
                    lUser.UserName = value.value.userData.name;
                    lUser.IsAdmin = lUser.Roles?.indexOf("Administrators") != -1;
                }
                this.idpStore.update(state => ({
                    LoggedInUser: lUser
                }));
            });
    }

}