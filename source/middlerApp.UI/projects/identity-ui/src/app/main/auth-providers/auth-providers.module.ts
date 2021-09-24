import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";
import { GlobalImportsModule } from "@admin/global-imports.module";
import { AuthProvidersRoutingModule, RoutingComponents } from "./auth-providers-routing.module";


@NgModule({
    imports: [
        CommonModule,
        GlobalImportsModule,
        ReactiveFormsModule,
        AuthProvidersRoutingModule
    ],
    declarations: [
        ...RoutingComponents
    ]

})
export class AuthProvidersModule {

    
}