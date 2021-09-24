import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";
import { GlobalImportsModule } from "@admin/global-imports.module";
import { RoutingComponents, RolesRoutingModule } from "./roles-routing.module";
import { RolesManagerModule } from "../roles-manager/roles-manager.module";
import { UsersManagerModule } from "../users-manager/users-manager.module";


@NgModule({
    imports: [
        CommonModule,
        GlobalImportsModule,
        ReactiveFormsModule,
        RolesRoutingModule,
        UsersManagerModule
    ],
    declarations: [
        ...RoutingComponents
    ]

})
export class RolesModule {

    
}