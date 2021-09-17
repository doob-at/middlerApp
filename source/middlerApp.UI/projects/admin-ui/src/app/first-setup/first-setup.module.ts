import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { FirstSetupComponent } from './first-setup.component';
import { FirstSetupService } from './first-setup.service';
import { GlobalImportsModule } from '@admin/global-imports.module';





const routes: Routes = [
    {
        path: '',
        component: FirstSetupComponent
    }
];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(routes),
        ReactiveFormsModule,
        GlobalImportsModule
    ],
    declarations: [
        FirstSetupComponent
    ],
    exports: [
    ],
    providers: [
        FirstSetupService
    ]
})
export class FirstSetupModule {

}
