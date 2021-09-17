import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";

import { NzInputModule } from 'ng-zorro-antd/input';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzInputNumberModule } from 'ng-zorro-antd/input-number';
import { NzCheckboxModule } from 'ng-zorro-antd/checkbox';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzMenuModule } from 'ng-zorro-antd/menu';

import { NzTabsModule } from 'ng-zorro-antd/tabs';
// import { IconsProviderModule } from './icons-provider.module';
import { NzCollapseModule } from 'ng-zorro-antd/collapse';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzTreeModule } from 'ng-zorro-antd/tree';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzToolTipModule } from 'ng-zorro-antd/tooltip';
import { DoobCoreModule } from '@doob-ng/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NzDrawerModule } from 'ng-zorro-antd/drawer';
import { DragDropModule } from "@angular/cdk/drag-drop";
import { DoobAntdExtensionsModule } from "@doob-ng/antd-extensions";
import { NzCalendarModule } from 'ng-zorro-antd/calendar';
import { OverlayModule } from '@angular/cdk/overlay';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { NzPopoverModule } from 'ng-zorro-antd/popover';
import { NzElementPatchModule } from 'ng-zorro-antd/core/element-patch'
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzResizableModule } from 'ng-zorro-antd/resizable';
import { AgGridModule } from "@ag-grid-community/angular";
import { DoobGridModule } from "@doob-ng/grid";
import { DoobEditorModule } from "@doob-ng/editor";

const GlobalModules = [
    NzFormModule,
    NzInputModule,
    NzButtonModule,
    NzInputNumberModule,
    NzCheckboxModule,
    NzSelectModule,
    NzDropDownModule,
    NzLayoutModule,
    NzMenuModule,

    NzTabsModule,
    // IconsProviderModule,
    NzCollapseModule,
    NzDatePickerModule,
    NzTreeModule,
    NzGridModule,
    NzToolTipModule,
    NzDrawerModule,
    NzCalendarModule,
    NzRadioModule,
    NzPopoverModule,
    NzElementPatchModule,
    NzDividerModule,
    NzResizableModule,
    
    DoobCoreModule,
    DoobAntdExtensionsModule,
    DoobGridModule,
    DoobEditorModule,

    FontAwesomeModule,

    DragDropModule,
    OverlayModule,

    AgGridModule
]

@NgModule({
    imports: [
        CommonModule,
        ...GlobalModules
    ],
    declarations: [
        
    ],
    exports: [
        CommonModule,
        ...GlobalModules
    ],
    providers: [
        
    ]
})
export class GlobalImportsModule {}