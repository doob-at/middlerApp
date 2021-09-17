import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";
import { GlobalImportsModule } from "@admin/global-imports.module";
import { RolesManagerComponent } from './roles-manager.component';
import { AddRolesListComponent } from './add-roles-list.component';
import { RoleGridCellComponent } from './role-grid-cell.component';

@NgModule({
    imports: [
        CommonModule,
        GlobalImportsModule,
        ReactiveFormsModule
    ],
    declarations: [
        RolesManagerComponent,
        AddRolesListComponent,
        RoleGridCellComponent
    ],
    exports: [
        RolesManagerComponent
    ]

})
export class RolesManagerModule {

    
}