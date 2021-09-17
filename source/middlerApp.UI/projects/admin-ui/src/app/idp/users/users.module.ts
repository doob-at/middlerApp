import { CommonModule } from "@angular/common";
import { HTTP_INTERCEPTORS } from "@angular/common/http";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule } from "@angular/forms";
import { AuthInterceptor } from "angular-auth-oidc-client";
import { GlobalImportsModule } from "@admin/global-imports.module";
import { ClaimsManagerModule } from "../claims-manager/claims-manager.module";
import { RolesManagerModule } from "../roles-manager/roles-manager.module";
import { SetPasswordModalComponent } from "./set-password-modal.component";
import { UsersRoutingModule, RoutingComponents } from "./users-routing.module";


@NgModule({
    imports: [
        CommonModule,
        GlobalImportsModule,
        ReactiveFormsModule,
        UsersRoutingModule,
        ClaimsManagerModule,
        RolesManagerModule
    ],
    declarations: [
        ...RoutingComponents,
        SetPasswordModalComponent
    ],
    providers: [
        { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
    ]

})
export class UsersModule {

    
}