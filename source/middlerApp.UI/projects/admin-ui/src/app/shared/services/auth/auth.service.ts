import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { LoggedInUser } from "../../models/logged-in-user";
import { AuthQuery, AuthStore } from "./auth.store";
import { OAuthService, UserInfo, LoginOptions, AuthConfig } from 'angular-oauth2-oidc';
import { filter } from "rxjs/operators";
import { AdminUIConfig, AdminUIConfigService } from "../../../admin-ui-config.service";

@Injectable({
    providedIn: 'root'
})
export class AuthService {

    private currentUserSubject$ = new BehaviorSubject<LoggedInUser | null>(null);
    currentUser$ = this.currentUserSubject$.asObservable();
    private authConfig: AuthConfig;
    private adminUIConfig: AdminUIConfig;
    
    constructor(private oauthService: OAuthService, private authStore: AuthStore, private adminUIConfigService: AdminUIConfigService) {

        this.adminUIConfig = adminUIConfigService.getConfiguration();

        this.authConfig = {

            issuer: this.adminUIConfig.IDPBaseUri,
            redirectUri: window.location.origin,
            clientId: 'mAdmin',
            scope: 'openid roles offline_access IdentityServerApi',
            postLogoutRedirectUri: window.location.origin,
            clearHashAfterLogin: true,
            silentRefreshRedirectUri: window.location.origin + '/api/account/silentRefresh',
            showDebugInformation: true,
            sessionChecksEnabled: false,
            responseType: 'code'
        };

        this.configureWithNewConfigApi()
    }

    private configureWithNewConfigApi() {


        this.oauthService.configure(this.authConfig);
        // this.oauthService.setStorage(localStorage);
        //this.oauthService.tokenValidationHandler = new JwksValidationHandler();


        this.oauthService.events.subscribe(e => {
            console.debug('oauth/oidc event', e);
        });

        this.oauthService.events.pipe(
            filter(e => e.type === 'session_terminated')
        ).subscribe(e => {
            console.debug('Your session has been terminated!');
        });

        this.oauthService.events.pipe(
            filter(e => e.type === 'token_received')
        ).subscribe(e => {
            this.oauthService.loadUserProfile().then((userInfo) => {
                this.setUser(<UserInfo>userInfo);
            });
        });

        this.oauthService.loadDiscoveryDocumentAndTryLogin().catch(err => { console.error(err); })
            .then(result => {

                if (this.oauthService.hasValidAccessToken()) {
                    this.oauthService.loadUserProfile().then((userInfo) => {
                        this.setUser(<UserInfo>userInfo);
                    });
                } else {
                    this.clearUser();
                }
            });

        // Optional
        this.oauthService.setupAutomaticSilentRefresh();


    }

    private setUser(userInfo: UserInfo) {

        const loggedInUser = new LoggedInUser();
        if (userInfo != null) {
            loggedInUser.UserName = userInfo.name;
            loggedInUser.Email = userInfo.email;
            loggedInUser.Roles = userInfo.role || [];
            loggedInUser.FirstName = userInfo.given_name;
            loggedInUser.LastName = userInfo.family_name;
            loggedInUser.DisplayName = userInfo.fullname;
        }


        this.currentUserSubject$.next(loggedInUser);

    }

    private clearUser() {
        this.currentUserSubject$.next(new LoggedInUser());
    }

    // constructor(private http: HttpClient, private authStore: AuthStore) {
    //     this.GetLoggedInUser();
    // }

    // GetLoggedInUser() {
    //     this.http.get<LoggedInUser>("/_api/status/userinfo").subscribe(user => {
    //         this.authStore.update(state => state = {
    //             LoggedInUser: user
    //         })
    //     });
    // }

    LogIn() {
        this.oauthService.initCodeFlow();
    }

    LogOut() {
        //this.clearUser();
        this.oauthService.logOut()
    }

}