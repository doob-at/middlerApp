import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";
import { GlobalImportsModule } from "@admin/global-imports.module";
import { UsersManagerComponent } from "./users-manager.component";
import { AddUsersListComponent } from "./add-users-list.component";


@NgModule({
    imports: [
        CommonModule,
        GlobalImportsModule,
        ReactiveFormsModule
    ],
    declarations: [
        UsersManagerComponent,
        AddUsersListComponent
    ],
    exports: [
        UsersManagerComponent
    ]

})
export class UsersManagerModule {

    
}