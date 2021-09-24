import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ClientListItem } from "./client-list-item";
import { ClientsStore } from "./clients.store";

@Injectable({
    providedIn: 'any'
})
export class ClientsService {


    constructor(private http:HttpClient, private adminStore: ClientsStore) {

    }


    GetApplications() {

        this.http.get<Array<ClientListItem>>("/_api/idp/clients").subscribe(data => {
            this.adminStore.set(data)
        })
    }

    
}