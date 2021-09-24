import { Component, ChangeDetectionStrategy } from "@angular/core";
import { ICellRendererAngularComp } from '@ag-grid-community/angular';
import { ICellRendererParams, IAfterGuiAttachedParams, RowNode } from '@ag-grid-community/core';
import { BehaviorSubject } from 'rxjs';

@Component({
    selector: 'icon-cell',
    templateUrl: './icon-cell.component.html',
    styleUrls: ['./icon-cell.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IconGridCellComponent implements ICellRendererAngularComp {
    
    iconSubject$ = new BehaviorSubject<string|null>(null);

    private onClick?: ((event: MouseEvent, node: RowNode) => void);

    private _params!: any;

    agInit(params: ICellRendererParams): void {

        this._params = params;

        const ico = this._params['icon'];
        if (ico !== undefined && ico != null) {
            this.iconSubject$.next(ico);
        } else {
            this.iconSubject$.next(params.value);
        }

        const onClick = this._params['onClick']
        if (onClick !== undefined && onClick != null) {
            this.onClick = onClick
        } 

    }

    public Click($event: MouseEvent) {
        if (this.onClick) {
            return this.onClick($event, this._params.node);
        } 
    }

    refresh(params: any): boolean {
        return false;
    }

    afterGuiAttached?(params?: IAfterGuiAttachedParams): void {
        
    }

}