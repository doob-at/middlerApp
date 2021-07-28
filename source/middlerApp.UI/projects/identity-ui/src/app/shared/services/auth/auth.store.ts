import { Injectable } from '@angular/core';
import { StoreConfig, Query, Store } from '@datorama/akita';
import { LoggedInUser } from '../../models/logged-in-user';


interface AuthState {
    LoggedInUser: LoggedInUser;
}

function createInitialState(): AuthState {
    return {
        LoggedInUser: new LoggedInUser()
    };
}


@Injectable({ providedIn: 'root' })
@StoreConfig({ name: 'auth' })
export class AuthStore extends Store<AuthState> {
    constructor() {
        super(createInitialState());
    }
}

@Injectable({
    providedIn: 'root'
})
export class AuthQuery extends Query<AuthState> {

    public loggedInUser$ = this.select(state => state.LoggedInUser);

    constructor(protected store: AuthStore) {
        super(store);
    }
}