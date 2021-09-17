import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MainComponent } from './main.component';
import { TestComponent } from './test/test.component';

const routes: Routes = [
  {
      path: '',
      children: [
          {
              path: '',
              component: MainComponent
          },
          {
            path: 'test',
            component: TestComponent
        }
      ]
  }

];


export const RoutingComponents = [
  MainComponent,
  TestComponent
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MainRoutingModule { }
