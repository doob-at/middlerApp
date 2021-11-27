import { StoreConfig, EntityState, EntityStore, QueryEntity } from '@datorama/akita';
import { Injectable } from '@angular/core';
import { EndpointRuleListDto } from './models/endpoint-rule-list-dto';


export interface EndpointRulesState extends EntityState<EndpointRuleListDto, string> { }

@Injectable({ providedIn: 'root' })
@StoreConfig({ name: 'endpoint-rules', idKey: 'Id' })
export class EndpointRulesStore extends EntityStore<EndpointRulesState> {
    constructor() {
        super();
    }
}

@Injectable({
    providedIn: 'root'
})
export class EndpointRulesQuery extends QueryEntity<EndpointRulesState> {
    constructor(protected store: EndpointRulesStore) {
        super(store);
    }
}