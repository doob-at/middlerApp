import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoggedOutComponent } from './logged-out/logged-out.component';
import { LoginComponent } from './login/login.component';
import { LogoutComponent } from './logout/logout.component';
import { TestComponent } from './test/test.component';

const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./main/main.module').then(m => m.MainModule)
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'logout',
    component: LogoutComponent
  },
  {
    path: 'logged-out',
    component: LoggedOutComponent
  }
];

export const RoutingComponents = [
  LoginComponent,
  LogoutComponent,
  LoggedOutComponent
]

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
