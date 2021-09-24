import { Store, StoreConfig, EntityState, EntityStore, QueryEntity } from '@datorama/akita';
import { MRoleDto } from '../models/m-role-dto';
import { Injectable } from '@angular/core';


export interface RolesState extends EntityState<MRoleDto, string> { }

@Injectable({ providedIn: 'root' })
@StoreConfig({ name: 'roles', idKey: 'Id' })
export class RolesStore extends EntityStore<RolesState> {
    constructor() {
        super();
    }
}

@Injectable({
    providedIn: 'root'
})
export class RolesQuery extends QueryEntity<RolesState, MRoleDto> {
    constructor(protected store: RolesStore) {
        super(store);
    }
}