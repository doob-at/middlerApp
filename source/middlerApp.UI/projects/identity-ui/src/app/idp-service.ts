import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ExternalLoginModel } from "./models/external-login-model";
import { LoginInputModel } from "./models/login-input-model";
import { LoginViewModel } from "./models/login-view-model";


@Injectable({
    providedIn: 'root'
})
export class IdpService {

    constructor(
        private http: HttpClient) {

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

}