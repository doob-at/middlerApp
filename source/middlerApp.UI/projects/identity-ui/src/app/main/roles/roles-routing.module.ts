import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { RoleDetailsComponent } from './role-details.component';
import { RolesComponent } from './roles.component';



const routes: Routes = [
    { path: '', component: RolesComponent },
    { path: ':id', component: RoleDetailsComponent}
];

export const RoutingComponents = [
    RolesComponent,
    RoleDetailsComponent
]

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class RolesRoutingModule { }