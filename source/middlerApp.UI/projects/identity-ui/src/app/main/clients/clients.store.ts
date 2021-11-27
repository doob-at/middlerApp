import { Injectable } from '@angular/core';
import { StoreConfig, Query, Store, EntityState, EntityStore, QueryEntity } from '@datorama/akita';
import { MClientListDto } from './models/MClientListDto';




interface ClientsState extends EntityState<MClientListDto, string> {
    
}


@Injectable({ providedIn: 'any' })
@StoreConfig({ name: 'clients',idKey: 'ClientId' })
export class ClientsStore extends EntityStore<ClientsState> {
    constructor() {
        super();
    }
}

@Injectable({
    providedIn: 'any'
})
export class ClientsQuery extends QueryEntity<ClientsState> {

    public applications$ = this.select(state => state.entities);

    constructor(protected store: ClientsStore) {
        super(store);
    }
}