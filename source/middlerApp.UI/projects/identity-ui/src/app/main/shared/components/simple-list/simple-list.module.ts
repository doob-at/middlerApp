import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";
import { GlobalImportsModule } from "@admin/global-imports.module";
import { SimpleListComponent } from "./simple-list.component";

@NgModule({
    imports: [
        CommonModule,
        GlobalImportsModule,
        ReactiveFormsModule
    ],
    declarations: [
        SimpleListComponent
    ],
    exports: [
        SimpleListComponent
    ]

})
export class SimpleListModule {

    
}