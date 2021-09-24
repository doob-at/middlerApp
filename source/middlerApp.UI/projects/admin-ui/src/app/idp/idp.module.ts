import { CommonModule } from "@angular/common";
import { HTTP_INTERCEPTORS } from "@angular/common/http";
import { NgModule } from "@angular/core";
import { AuthInterceptor } from "angular-auth-oidc-client";
import { GlobalImportsModule } from "../global-imports.module";

import { IdpRoutingModule, RoutingComponents } from "./idp-routing.module";
import { IconGridCellComponent } from "./shared/components/icon-cell.component";



@NgModule({
    imports: [
        CommonModule,
        GlobalImportsModule,
        IdpRoutingModule
    ],
    declarations: [
        ...RoutingComponents,
        IconGridCellComponent
    ]
})
export class IdpModule {

}