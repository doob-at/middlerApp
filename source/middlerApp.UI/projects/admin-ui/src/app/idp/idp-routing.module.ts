import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AuthApplicationsComponent } from './applications/auth-applications.component';



const routes: Routes = [
    {
        path: '',
        children: [
            {
                path: 'users',
                loadChildren: () => import('./users/users.module').then(m => m.UsersModule)
            },
            {
                path: 'roles',
                loadChildren: () => import('./roles/roles.module').then(m => m.RolesModule)
            },
            {
                path: 'auth-providers',
                loadChildren: () => import('./auth-providers/auth-providers.module').then(m => m.AuthProvidersModule)
            },
            {
                path: 'applications',
                component: AuthApplicationsComponent
            }
        ]
    }

];


export const RoutingComponents = [
    AuthApplicationsComponent,
]

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class IdpRoutingModule { }
