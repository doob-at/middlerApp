import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";
import { GlobalImportsModule } from "@admin/global-imports.module";
import { RoutingComponents, RolesRoutingModule } from "./roles-routing.module";


@NgModule({
    imports: [
        CommonModule,
        GlobalImportsModule,
        ReactiveFormsModule,
        RolesRoutingModule
    ],
    declarations: [
        ...RoutingComponents
    ]

})
export class RolesModule {

    
}