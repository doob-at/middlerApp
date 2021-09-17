import { Injectable } from '@angular/core';
import { StoreConfig, Query, Store, EntityState, EntityStore, QueryEntity } from '@datorama/akita';
import { AuthApplicationListItem } from './auth-application-list-item';



interface AuthApplicationState extends EntityState<AuthApplicationListItem, string> {
    
}


@Injectable({ providedIn: 'any' })
@StoreConfig({ name: 'auth-applications',idKey: 'ClientId' })
export class AuthApplicationStore extends EntityStore<AuthApplicationState> {
    constructor() {
        super();
    }
}

@Injectable({
    providedIn: 'any'
})
export class AuthApplicationQuery extends QueryEntity<AuthApplicationState> {

    public applications$ = this.select(state => state.entities);

    constructor(protected store: AuthApplicationStore) {
        super(store);
    }
}