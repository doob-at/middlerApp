import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";
import { GlobalImportsModule } from "@admin/global-imports.module";
import { ClientsRoutingModule, RoutingComponents } from "./clients-routing.module";
import { SimpleListModule } from "../shared/components/simple-list/simple-list.module";


@NgModule({
    imports: [
        CommonModule,
        GlobalImportsModule,
        ReactiveFormsModule,
        ClientsRoutingModule,
        SimpleListModule
    ],
    declarations: [
        ...RoutingComponents
    ]

})
export class ClientsModule {

    
}