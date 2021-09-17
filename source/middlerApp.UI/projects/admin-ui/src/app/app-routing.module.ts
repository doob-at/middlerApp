import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  // {
  //   path: 'endpoint-rules',
  //   loadChildren: () => import('./endpoint-rules/endpoint-rules.module').then(m => m.EndpointRulesModule)
  // },
  // {
  //   path: 'global-variables',
  //   loadChildren: () => import('./global-variables/global-variables.module').then(m => m.GlobalVariablesModule)
  // },
  {
    path: 'idp',
    loadChildren: () => import('./idp/idp.module').then(m => m.IdpModule)
  },
  {
    path: 'first-setup',
    loadChildren: () => import('./first-setup/first-setup.module').then(m => m.FirstSetupModule)
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
