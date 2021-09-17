import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { AuthApplicationStore } from "./auth-application.store";
import { AuthApplicationListItem } from "./auth-application-list-item";

@Injectable({
    providedIn: 'any'
})
export class AuthApplicationService {


    constructor(private http:HttpClient, private adminStore: AuthApplicationStore) {

    }


    GetApplications() {

        this.http.get<Array<AuthApplicationListItem>>("/_api/idp/applications").subscribe(data => {
            this.adminStore.set(data)
        })
    }

    
}