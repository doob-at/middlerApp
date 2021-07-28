import { NgModule } from "@angular/core";
import { CommonModule } from '@angular/common';
import { MainRoutingModule, RoutingComponents } from './main-routing.module';
import { NgZorroImportsModule } from "../ng-zorro-imports.module";


@NgModule({
    imports: [
        CommonModule,
        MainRoutingModule,
        NgZorroImportsModule
    ],
    declarations: [
        ...RoutingComponents
    ]
})
export class MainModule {

}