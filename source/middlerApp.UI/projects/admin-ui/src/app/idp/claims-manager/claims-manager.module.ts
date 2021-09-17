import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { GlobalImportsModule } from "@admin/global-imports.module";
import { ClaimsManagerComponent } from "./claims-manager.component";

@NgModule({
    imports: [
        CommonModule,
        GlobalImportsModule
    ],
    declarations: [
        ClaimsManagerComponent
    ],
    exports: [
        ClaimsManagerComponent
    ]
})
export class ClaimsManagerModule {

}