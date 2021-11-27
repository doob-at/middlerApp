import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ClientDetailsComponent } from './client-details.component';
import { ClientsComponent } from './clients.component';



const routes: Routes = [
    { path: '', component: ClientsComponent },
    { path: ':id', component: ClientDetailsComponent}
    
];

export const RoutingComponents = [
    ClientsComponent,
    ClientDetailsComponent
]

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ClientsRoutingModule { }