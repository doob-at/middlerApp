import { StoreConfig, EntityState, EntityStore, QueryEntity } from '@datorama/akita';
import { Injectable } from '@angular/core';
import { MUserListDto } from './models/m-user-list-dto';


export interface UsersState extends EntityState<MUserListDto, string> { }

@Injectable({ providedIn: 'root' })
@StoreConfig({ name: 'identity-users', idKey: 'Id' })
export class UsersStore extends EntityStore<UsersState> {
    constructor() {
        super();
    }
}

@Injectable({
    providedIn: 'root'
})
export class UsersQuery extends QueryEntity<UsersState> {
    constructor(protected store: UsersStore) {
        super(store);
    }
}