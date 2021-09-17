import { StoreConfig, EntityState, EntityStore, QueryEntity } from '@datorama/akita';
import { Injectable } from '@angular/core';
import { AuthProviderDto } from './models/auth-provider-dto';





export interface AuthProvidersState extends EntityState<AuthProviderDto, string> { }

@Injectable({ providedIn: 'root' })
@StoreConfig({ name: 'authentication-providers', idKey: 'Id' })
export class AuthProvidersStore extends EntityStore<AuthProvidersState> {
    constructor() {
        super();
    }
}

@Injectable({
    providedIn: 'root'
})
export class AuthProvidersQuery extends QueryEntity<AuthProvidersState> {
    constructor(protected store: AuthProvidersStore) {
        super(store);
    }
}