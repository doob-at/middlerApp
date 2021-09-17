import { IconsImport } from "@admin/icons-import";
import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FaConfig, FaIconLibrary } from "@fortawesome/angular-fontawesome";
import { GlobalImportsModule } from "./global-imports.module";
import { MainRoutingModule, RoutingComponents } from "./main-routing.module";
import { MainComponent } from "./main.component";

@NgModule({
    imports: [
        CommonModule,
        MainRoutingModule,
        GlobalImportsModule
    ],
    declarations: [
        MainComponent,
        ...RoutingComponents
    ]
})
export class MainModule {

    constructor(private library: FaIconLibrary, faConfig: FaConfig) {

        var fLib = new IconsImport();
        fLib.Init(library);
    
      }
}