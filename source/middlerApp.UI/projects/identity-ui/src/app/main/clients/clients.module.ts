import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";
import { GlobalImportsModule } from "@admin/global-imports.module";
import { ClientsRoutingModule, RoutingComponents } from "./clients-routing.module";


@NgModule({
    imports: [
        CommonModule,
        GlobalImportsModule,
        ReactiveFormsModule,
        ClientsRoutingModule
    ],
    declarations: [
        ...RoutingComponents
    ]

})
export class ClientsModule {

    
}