
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
import { NzCollapseModule } from 'ng-zorro-antd/collapse';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzTreeModule } from 'ng-zorro-antd/tree';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzToolTipModule } from 'ng-zorro-antd/tooltip';
import { NzDrawerModule } from 'ng-zorro-antd/drawer';
import { DragDropModule } from "@angular/cdk/drag-drop";
import { NgModule } from '@angular/core';
import { NzCalendarModule } from 'ng-zorro-antd/calendar';
import { CommonModule } from '@angular/common';
import { OverlayModule } from '@angular/cdk/overlay';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { NzPopoverModule } from 'ng-zorro-antd/popover';
import { NzElementPatchModule } from 'ng-zorro-antd/core/element-patch'
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzResizableModule } from 'ng-zorro-antd/resizable';

import { DoobAntdExtensionsModule } from '@doob-ng/antd-extensions';
import { DoobCoreModule } from '@doob-ng/core';

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

    DragDropModule,
    OverlayModule,

    DoobAntdExtensionsModule,
    DoobCoreModule

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
export class NgZorroImportsModule {}