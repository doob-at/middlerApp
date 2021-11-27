import { HttpClient, HttpHeaders, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { tap } from "rxjs/operators";
import { ExternalLoginModel } from "./models/external-login-model";
import { LogOutModel } from "./models/log-out-model";
import { LoginInputModel } from "./models/login-input-model";
import { LoginViewModel } from "./models/login-view-model";


@Injectable({
    providedIn: 'root'
})
export class IdpService {

    logOutModel!: LogOutModel;

    constructor(
        private http: HttpClient,
        private router: Router) {

    }

    public GetLoginViewModel(returnUrl: string) {
        let params: HttpParams;
        if (returnUrl) {
            params = new HttpParams().set('returnUrl', returnUrl);
        } else {
            params = new HttpParams();
        }

        return this.http.get<LoginViewModel>(`/_idp/account/login`, { params });
    }

    public SendLoginInputModel(loginInputModel: LoginInputModel) {
        return this.http.post<any>(`/_idp/account/login`, loginInputModel);
    }

    public SendExternalLoginInputModel(loginModel: ExternalLoginModel) {

        return this.http.post<any>(`/_idp/account/login-external`, loginModel);
    }


    public GetLogoutViewModel(queryString: string) {
        
        return this.http.get<LogOutModel>(`/connect/logout${queryString}`)
        .pipe(
            tap(model => {

                this.logOutModel = model;
                if(!this.logOutModel?.ShowLogoutPrompt){
                    this.router.navigate(['/logged-out']);
                }
                
            })
        );
    }

    public CompleteLogOut() {
        
        var headers = new HttpHeaders({
            'Content-Type': 'application/x-www-form-urlencoded',
        });

        return this.http.post(`/connect/logout`, this.logOutModel, {headers: headers});
    }

}