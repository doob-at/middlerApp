import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MainComponent } from './main.component';
import { TestComponent } from '../test/test.component';
import { DashboardComponent } from './dashboard/dashboard.component';

const routes: Routes = [
  {
    path: '',
    component: MainComponent,
    children: [
      {
        path: '',
        component: DashboardComponent
      },
      {
        path: 'test',
        component: TestComponent
      },
      {
        path: 'users',
        loadChildren: () => import('./users/users.module').then(m => m.UsersModule)
      },
      {
        path: 'roles',
        loadChildren: () => import('./roles/roles.module').then(m => m.RolesModule)
      },
      {
        path: 'clients',
        loadChildren: () => import('./clients/clients.module').then(m => m.ClientsModule)
      },
      {
        path: 'auth-providers',
        loadChildren: () => import('./auth-providers/auth-providers.module').then(m => m.AuthProvidersModule)
      }
    ]
  }

];


export const RoutingComponents = [
  MainComponent,
  TestComponent,
  DashboardComponent
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MainRoutingModule { }
