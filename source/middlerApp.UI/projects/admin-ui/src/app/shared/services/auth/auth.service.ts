import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { LoggedInUser } from "../../models/logged-in-user";
import { AuthQuery, AuthStore } from "./auth.store";

@Injectable({
    providedIn: 'root'
})
export class AuthService {


    constructor(private http: HttpClient, private authStore: AuthStore) {
        this.GetLoggedInUser();
    }

    GetLoggedInUser() {
        this.http.get<LoggedInUser>("/_api/status/userinfo").subscribe(user => {
            this.authStore.update(state => state = {
                LoggedInUser: user
            })
        });
    }

}