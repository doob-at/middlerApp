import { GlobalImportsModule } from "@admin/global-imports.module";
import { PermissionManagerModule } from "@admin/permissions-manager/permission-manager.module";
import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";

import { UrlRewriteModalComponent, UrlRedirectModalComponent, ScriptModalComponent } from "./endpoint-actions";
import { ActionListDetailsComponent } from "./endpoint-actions-list/action-list-details/action-list-details.component";
import { EndpointActionsListComponent } from "./endpoint-actions-list/endpoint-actions-list.component";
import { ActionBasicModalComponent } from "./endpoint-actions/base/action-basic-modal.component";
import { EndpointRulesListComponent } from "./endpoint-rules-list/endpoint-rules-list.component";
import { EndpointRulesRoutingModule, RoutingComponents } from "./endpoint-rules-routing.module";

const ActionComponents = [
    ActionBasicModalComponent,
    UrlRewriteModalComponent, UrlRedirectModalComponent, ScriptModalComponent
]

@NgModule({
    imports: [
        CommonModule,
        GlobalImportsModule,
        EndpointRulesRoutingModule,
        PermissionManagerModule,
        ReactiveFormsModule
       
    ],
    declarations: [
        EndpointActionsListComponent,
        ActionListDetailsComponent,
        EndpointRulesListComponent,
        ...RoutingComponents,
        ...ActionComponents
    ]
})
export class EndpointRulesModule {

    
}