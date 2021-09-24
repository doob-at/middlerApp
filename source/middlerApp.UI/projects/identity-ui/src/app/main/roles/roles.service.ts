import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { MessageService } from "@shared/services";
import { OidcSecurityService } from "angular-auth-oidc-client";
import { tap } from "rxjs/operators";
import { DataEvent } from "@shared/models/data-event";
import { MRoleDto } from "../models/m-role-dto";
import { RolesStore } from "./roles.store";



@Injectable({
    providedIn: 'any'
})
export class RolesService {


    constructor(
        private http: HttpClient,
        private oidcSecurityService: OidcSecurityService,
        private messageService: MessageService,
        private roleStore:RolesStore) {

        this.SubscribeRoleEvents();

        this.ReLoadRoles()

        this.messageService.RunOnEveryReconnect(() => this.ReLoadRoles());

    }

    getHeaders() {
        return new HttpHeaders({
            //'Authorization': 'Bearer ' + this.oidcSecurityService.getAccessToken()
        });
    }

    //#region Roles

    SubscribeRoleEvents() {
        console.log("IDPRoles.Subscribe")
        this.messageService.Stream<DataEvent<MRoleDto>>("IDPRoles.Subscribe").pipe(
            tap(item => {
                console.log(item)
                switch (item.Action) {
                    case "Created": {
                        this.roleStore.add(item.Payload);
                        break;
                    }
                    case "Updated": {
                        this.roleStore.update(item.Payload.Id, entity => item.Payload);
                        break;
                    }
                    case "Deleted": {
                        this.roleStore.update(<any>item.Payload, entity => ({
                            ...entity,
                            Deleted: true
                        }));
                        //this.identityRolesStore.remove(<any>item.Payload)
                    }
                }

            })
        ).subscribe()
    }
    ReLoadRoles() {
        this.http.get<Array<MRoleDto>>(`_api/idp/roles`, { headers: this.getHeaders() }).pipe(
            tap(users => this.roleStore.set(users))
        ).subscribe();
    }
    
    GetRole(id: string) {
        return this.http.get<MRoleDto>(`_api/idp/roles/${id}`, { headers: this.getHeaders() })
    }

    CreateRole(roleModel: MRoleDto) {
        return this.http.post(`_api/idp/roles`, roleModel, { headers: this.getHeaders() });
    }

    UpdateRole(roleModel: MRoleDto) {
        return this.http.put(`_api/idp/roles`, roleModel, { headers: this.getHeaders() });
    }

    DeleteRole(...ids: string[]) {

        const options = {
            body: ids,
            headers: this.getHeaders()
        }

        return this.http.request("delete", `_api/idp/roles`, options);
    }
    //#endregion


}