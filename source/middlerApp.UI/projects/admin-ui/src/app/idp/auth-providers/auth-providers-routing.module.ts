import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AuthProviderDetailsComponent } from './auth-provider-details.component';
import { AuthProvidersComponent } from './auth-providers.component';



const routes: Routes = [
    { path: '', component: AuthProvidersComponent },
    { path: ':id', component: AuthProviderDetailsComponent}
];

export const RoutingComponents = [
    AuthProvidersComponent,
    AuthProviderDetailsComponent
]

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class AuthProvidersRoutingModule { }