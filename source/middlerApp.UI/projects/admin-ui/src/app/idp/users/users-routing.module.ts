import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { UserDetailsComponent } from './user-details.component';
import { UsersComponent } from './users.component';



const routes: Routes = [
    { path: '', component: UsersComponent },
    { path: ':id', component: UserDetailsComponent}
];

export const RoutingComponents = [
    UsersComponent,
    UserDetailsComponent
]

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class UsersRoutingModule { }