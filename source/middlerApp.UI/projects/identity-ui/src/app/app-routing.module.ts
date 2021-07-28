import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { IdpUILoginComponent } from './login/login.component';

const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./main/main.module').then(m => m.MainModule)
  },
  {
    path: 'login',
    component: IdpUILoginComponent
  }
];

export const RoutingComponents = [
  IdpUILoginComponent,
  // IdpUIConsentComponent,
  // IdpErrorComponent,
  // IdpUILogoutComponent,
  // IdpUILoggedOutComponent
]

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
