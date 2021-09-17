import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { MessageService } from "@shared/services";
import { OidcSecurityService } from "angular-auth-oidc-client";
import { tap } from "rxjs/operators";
import { DataEvent } from "@shared/models/data-event";
import { AuthProvidersQuery, AuthProvidersStore } from "./auth-providers.store";
import { AuthProviderDto } from "./models/auth-provider-dto";


@Injectable({
    providedIn: 'any'
})
export class AuthProviderService {


    constructor(
        private http:HttpClient, 
        private oidcSecurityService: OidcSecurityService,
        private messageService: MessageService,
        private AuthProvidersStore: AuthProvidersStore,
        private AuthProvidersQuery: AuthProvidersQuery,) {

            this.SubscribeAuthProvidersEvents();

            this.ReLoadAuthProviders()

        this.messageService.RunOnEveryReconnect(() => this.ReLoadAuthProviders());

    }

    getHeaders() {
        return new HttpHeaders({
            //'Authorization': 'Bearer ' + this.oidcSecurityService.getAccessToken()
        });
    }

    //#region AuthProviders

    SubscribeAuthProvidersEvents() {
        this.messageService.Stream<DataEvent<AuthProviderDto>>("AuthProviders.Subscribe").pipe(
            tap(item => {
                console.log(item)
                switch (item.Action) {
                    case "Created": {
                        this.AuthProvidersStore.add(item.Payload);
                        break;
                    }
                    case "Updated": {
                        this.AuthProvidersStore.update(item.Payload.Id, entity => item.Payload);
                        break;
                    }
                    case "Deleted": {
                        this.AuthProvidersStore.update(<any>item.Payload, entity => ({
                            ...entity,
                            Deleted: true
                        }));
                    }
                }

            })
        ).subscribe()
    }
    ReLoadAuthProviders() {
        this.http.get<Array<AuthProviderDto>>(`_api/idp/auth-provider`, { headers: this.getHeaders() }).pipe(
            tap(users => this.AuthProvidersStore.set(users))
        ).subscribe();
    }

    GetAuthProvider(id: string) {
        return this.http.get<AuthProviderDto>(`_api/idp/auth-provider/${id}`, { headers: this.getHeaders() })
    }

    CreateAuthProvider(dtoModel: AuthProviderDto) {
        return this.http.post(`_api/idp/auth-provider`, dtoModel, { headers: this.getHeaders() });
    }

    UpdateAuthProvider(dtoModel: AuthProviderDto) {
        return this.http.put(`_api/idp/auth-provider`, dtoModel, { headers: this.getHeaders() });
    }

    DeleteAuthProvider(...ids: string[]) {

        const options = {
            body: ids,
            headers: this.getHeaders()
        }

        return this.http.request("delete", `_api/idp/auth-provider`, options);
    }

    //#endregion

    
}