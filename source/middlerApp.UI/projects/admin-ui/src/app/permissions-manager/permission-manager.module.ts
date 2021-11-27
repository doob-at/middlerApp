import { CommonModule } from '@angular/common';
import { NgModule } from "@angular/core";
import { ReactiveFormsModule } from '@angular/forms';

import { AddPermissionRuleComponent } from './add-rule-modal.component';
import { PermissionsManagerComponent } from './permissions-manager.component';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { GlobalImportsModule } from '@admin/global-imports.module';


@NgModule({
    imports: [
        CommonModule,
        ReactiveFormsModule,
        NzSelectModule,
        GlobalImportsModule
    ],
    exports: [
        PermissionsManagerComponent,
        AddPermissionRuleComponent,
    ],
    declarations: [
        PermissionsManagerComponent,
        AddPermissionRuleComponent,
    ]
})
export class PermissionManagerModule {

}