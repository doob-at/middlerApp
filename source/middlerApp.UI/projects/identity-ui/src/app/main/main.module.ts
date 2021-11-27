import { IconsImport } from "@admin/icons-import";
import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FaConfig, FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { NzButtonModule } from "ng-zorro-antd/button";
import { GlobalImportsModule } from "../global-imports.module";
import { MainRoutingModule, RoutingComponents } from "./main-routing.module";
import { MainComponent } from "./main.component";
import { IconGridCellComponent } from "./shared/components/icon-cell/icon-cell.component";

@NgModule({
    imports: [
        CommonModule,
        MainRoutingModule,
        GlobalImportsModule,
        NzButtonModule
    ],
    declarations: [
        MainComponent,
        IconGridCellComponent,
        ...RoutingComponents
    ]
})
export class MainModule {

    constructor(private library: FaIconLibrary, faConfig: FaConfig) {

        var fLib = new IconsImport();
        fLib.Init(library);
    
      }
}