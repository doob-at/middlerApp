import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { MessageService } from "@shared/services";
import { OidcSecurityService } from "angular-auth-oidc-client";
import { tap } from "rxjs/operators";
import { DataEvent } from "@shared/models/data-event";
import { SetPasswordDto } from "../models/set-password-dto";
import { MUserDto } from "./models/m-user-dto";
import { MUserListDto } from "./models/m-user-list-dto";
import { UsersStore } from "./users.store";


@Injectable({
    providedIn: 'any'
})
export class UsersService {


    constructor(
        private http:HttpClient, 
        private oidcSecurityService: OidcSecurityService,
        private messageService: MessageService,
        private usersStore: UsersStore) {

            //this.SubscribeUserEvents();

            this.ReLoadUsers()

        this.messageService.RunOnEveryReconnect(() => this.ReLoadUsers());

    }

    getHeaders() {
        return new HttpHeaders({
            //'Authorization': 'Bearer ' + this.oidcSecurityService.getAccessToken()
        });
    }

    //#region  Users
    SubscribeUserEvents() {
        this.messageService.Stream<DataEvent<MUserListDto>>("IDPUsers.Subscribe").pipe(
            tap(item => {
                console.log(item)
                switch (item.Action) {
                    case "Created": {
                        this.usersStore.add(item.Payload);
                        break;
                    }
                    case "Updated": {
                        this.usersStore.update(item.Payload.Id, entity => item.Payload);
                        break;
                    }
                    case "Deleted": {
                        this.usersStore.update(<any>item.Payload, entity => ({
                            ...entity,
                            Deleted: true
                        }));
                    }
                }

            })
        ).subscribe()
    }
    ReLoadUsers() {
        this.http.get<Array<MUserListDto>>(`_api/idp/users`, { headers: this.getHeaders() }).pipe(
            tap(users => this.usersStore.set(users))
        ).subscribe();
    }
    
    GetUser(id: string) {
        return this.http.get<MUserDto>(`_api/idp/users/${id}`, { headers: this.getHeaders() })
    }

    CreateUser(createUserModel: MUserDto) {
        return this.http.post(`_api/idp/users`, createUserModel, { headers: this.getHeaders() });
    }

    UpdateUser(createUserModel: MUserDto) {
        return this.http.put(`_api/idp/users`, createUserModel, { headers: this.getHeaders() });
    }

    DeleteUser(...ids: string[]) {

        const options = {
            body: ids,
            headers: this.getHeaders()
        }

        return this.http.request("delete", `_api/idp/users`, options);
    }


    SetPassword(userId: string, dto: SetPasswordDto) {
        return this.http.post(`_api/idp/users/${userId}/password`, dto, { headers: this.getHeaders() });
    }

    ClearPassword(userId: string) {
        return this.http.delete(`_api/idp/users/${userId}/password`, { headers: this.getHeaders() });
    }

    //#endregion

    
}