import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { EndpointRuleDetailsComponent } from './endpoint-rule-details/endpoint-rule-details.component';
import { EndpointRulesComponent } from './endpoint-rules.component';


const routes: Routes = [
    { path: '', component: EndpointRulesComponent },
    { path: ':id', component: EndpointRuleDetailsComponent}
];

export const RoutingComponents = [
    EndpointRulesComponent,
    EndpointRuleDetailsComponent
]

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class EndpointRulesRoutingModule { }