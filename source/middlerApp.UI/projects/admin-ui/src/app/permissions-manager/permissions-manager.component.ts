import { RowDragEndEvent, RowDragLeaveEvent, RowDragMoveEvent } from '@ag-grid-community/core';
import { Component, Input, Output, EventEmitter, TemplateRef, ViewContainerRef, ViewChild } from "@angular/core";
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { IOverlayHandle, DoobOverlayService, DoobModalService } from '@doob-ng/cdk-helper';
import { GridBuilder, DefaultContextMenuContext } from '@doob-ng/grid';
import { BehaviorSubject, Subject } from 'rxjs';
import { map, takeUntil, tap } from 'rxjs/operators';
import { AddPermissionRuleComponent } from './add-rule-modal.component';

import { PermissionEntry, PermissionEntrySortByOrder } from './permission-entry';

@Component({
    selector: 'permissions-manager',
    templateUrl: './permissions-manager.component.html',
    styleUrls: ['./permissions-manager.component.scss'],
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: PermissionsManagerComponent,
        multi: true
    }],
})
export class PermissionsManagerComponent implements ControlValueAccessor {


    _entries: Array<PermissionEntry> = [];

    @Input()
    set entries(value: Array<PermissionEntry>) {
        let cls = value || [];
        if (cls.length == 0) {
            cls = [];
        }
        this._entries = cls;
        this.grid?.SetData(this._entries);
    }
    get entries() {
        return this._entries;
    }

    private _disabled: boolean = false;
    @Input()
    get disabled() {
        return this._disabled;
    };
    set disabled(value: any) {
        if (value === null || value === undefined || value === false) {
            this._disabled = false
        } else {
            this._disabled = !!value
        }
    }

    @Output() entriesChange: EventEmitter<Array<PermissionEntry>> = new EventEmitter<Array<PermissionEntry>>();

    @ViewChild('itemsContextMenu') itemsContextMenu!: TemplateRef<any>;
    @ViewChild('viewportContextMenu') viewportContextMenu!: TemplateRef<any>

    grid = new GridBuilder<PermissionEntry>()
        .SetColumns(
            c => c.Default("Type")
                .SetInitialWidth(300, true)
                .Set(col => col.rowDrag = true)
                .Sortable(false),
            c => c.Default("PrincipalName").Sortable(false),
            c => c.Default("AccessMode").Sortable(false),
            c => c.Default("Client").Sortable(false),
            c => c.Default("SourceAddress").Sortable(false)
        )
        .WithRowSelection("multiple")
        .SetGridOptions({
            onRowDragMove: (ev) => this.onRowDragMove(ev),
            onRowDragEnd: (ev) => this.dragEnd(ev),
            onRowDragLeave: (ev) => this.onRowDragLeave(ev)
        })
        .WithFullRowEditType()
        .WithShiftResizeMode()
        .OnDataUpdate(data => this.propagateChange(data))
        .OnCellContextMenu(ev => {
            const selected = ev.api.getSelectedNodes();
            if (selected.length == 0 || !selected.includes(ev.node)) {
                ev.node.setSelected(true, true)
            }

            let vContext = new DefaultContextMenuContext(ev.api, ev.event as MouseEvent)
            this.contextMenu = this.overlay.OpenContextMenu(ev.event as MouseEvent, this.itemsContextMenu, this.viewContainerRef, vContext)
        })
        .OnViewPortContextMenu((ev, api) => {
            api.deselectAll();
            let vContext = new DefaultContextMenuContext(api, ev as MouseEvent)
            this.contextMenu = this.overlay.OpenContextMenu(ev, this.viewportContextMenu, this.viewContainerRef, vContext)
        })
        .OnRowDoubleClicked(el => {
            this.OpenEditVariableModal(el.data);
        })
        .StopEditingWhenCellsLoseFocus()
        .OnGridSizeChange(ev => ev.api.sizeColumnsToFit())
        .OnViewPortClick((ev, api) => {
            api.deselectAll();
        });

    private contextMenu: IOverlayHandle | null = null;

    constructor(
        public overlay: DoobOverlayService,
        public viewContainerRef: ViewContainerRef,
        private modal: DoobModalService,
    ) {

    }


    AddRule(): void {

        this.contextMenu?.Close();
        this.OpenEditVariableModal(null);
    }

    OpenEditVariableModal(pEntry: PermissionEntry | null) {

        var isNew = pEntry == null;
        pEntry = pEntry ?? new PermissionEntry();
        const modal = this.modal.FromComponent(AddPermissionRuleComponent)
            .SetData(pEntry)
            .SetModalOptions({
                overlayConfig: {
                    minWidth: 500,
                    // minHeight: 400
                }
            })
            .AddEventHandler<PermissionEntry>('OK', (context) => {

                if (isNew) {
                    context.payload.Order = this.GetNextLastOrder();
                    this.entries = [...this.entries, context.payload];
                } else {
                    this.entries = this.entries.map(e => {
                        if(e.Id == context.payload.Id) {
                            e.AccessMode = context.payload.AccessMode;
                            e.Client = context.payload.Client;
                            e.PrincipalName = context.payload.PrincipalName;
                            e.SourceAddress = context.payload.SourceAddress;
                            e.Type = context.payload.Type
                        }

                        return e;
                    })
                }

                context.handle.Close();
            });

        modal.Open();

    }

    RemoveClaim(arr: Array<PermissionEntry>): void {
        //let ids = arr.map(c => c.Id);
        this.entries = this.entries.filter(d => !arr.includes(d));
        this.contextMenu?.Close();
    }


    moveItem$!: Subject<RowDragMoveEvent> | null;
    cancelMoveItem$!: Subject<any> | null;
    dragStartingIndex!: number | null;
    dragItem!: PermissionEntry | null;

    onRowDragMove(ev: RowDragMoveEvent) {

        if (!this.moveItem$) {
            this.moveItem$ = new Subject<RowDragMoveEvent>();
            this.cancelMoveItem$ = new Subject<any>();
            this.dragStartingIndex = this.entries.indexOf(ev.node.data);
            this.dragItem = ev.node.data;

            this.moveItem$.pipe(
                takeUntil(this.cancelMoveItem$)
            ).subscribe(
                (event) => {
                    var movingNode = event.node;
                    var overNode = event.overNode;

                    var rowNeedsToMove = movingNode !== overNode;
                    if (rowNeedsToMove) {

                        var movingData = movingNode.data;
                        var overData = overNode?.data;
                        var fromIndex = this.entries.indexOf(movingData);
                        var toIndex = overData ? this.entries.indexOf(overData) : this.entries.length - 1;
                        var newStore = this.entries.slice();
                        moveInArray(newStore, fromIndex, toIndex);
                        this.entries = newStore;
                    }
                },
                (error) => console.log("Error", error),
                () => {
                    var fromIndex = this.entries.indexOf(this.dragItem!);
                    var newStore = this.entries.slice();
                    moveInArray(newStore, fromIndex, this.dragStartingIndex!);
                    this.entries = newStore;

                    this.moveItem$ = null;
                    this.cancelMoveItem$ = null;
                    this.dragStartingIndex = null;
                    this.dragItem = null;
                }
            )

        }


        this.moveItem$.next(ev);

        function moveInArray(arr: Array<any>, fromIndex: number, toIndex: number) {
            var element = arr[fromIndex];
            arr.splice(fromIndex, 1);
            arr.splice(toIndex, 0, element);
        }
    }

    onRowDragLeave(event: RowDragLeaveEvent) {
        this.cancelMoveItem$?.next(true);
    }

    private GetNextLastOrder() {
        if (!this.entries || this.entries.length === 0) {
            return 10;
        }
        var next = Math.trunc(Math.max(...this.entries.map(r => r.Order)) + 10);
        console.log("NEXT", next)
        return next;
    }

    dragEnd(event: RowDragEndEvent) {

        var newIndex = this.entries.indexOf(event.node.data);
        var length = this.entries.length;

        if (newIndex == this.dragStartingIndex) {
            console.log("Gleich")
        } else if (newIndex == this.entries.length - 1) {
            this.setOrder(this.dragItem!, this.GetNextLastOrder())
        } else if (newIndex == 0) {
            if (this.entries.length > 1) {
                newIndex = this.entries[1].Order / 2;
            }
            this.setOrder(this.dragItem!, newIndex)
        } else {
            console.log("Dazwischen")
            var before = this.entries[newIndex - 1].Order;
            var after = this.entries[newIndex + 1].Order;
            this.setOrder(this.dragItem!, ((after - before) / 2) + before)
        }


        this.moveItem$ = null;
        this.cancelMoveItem$ = null;
        this.dragStartingIndex = null;
        this.dragItem = null;
    }

    setOrder(entry: PermissionEntry, order: number) {
        entry.Order = order;
        this.entries = [...this.entries];

    }


    propagateChange(value: Array<PermissionEntry>) {

        this.entriesChange.emit(value);
        this.registered.forEach(fn => {
            fn(value);
        });
    }

    writeValue(value: Array<PermissionEntry>): void {
        this.entries = value;
    }

    private registered:Array<any> = [];
    registerOnChange(fn: any): void {
        if (this.registered.indexOf(fn) === -1) {
            this.registered.push(fn);
        }
    }

    onTouched = () => { };
    registerOnTouched(fn: any): void {
        this.onTouched = fn;
    }

    setDisabledState?(isDisabled: boolean): void {
        this.disabled = isDisabled;
    }

}

// export class entriesItemsContext<T = any> {

//     SelectedData: Array<T>;

//     get SelectedCount() {
//         return this.SelectedData.length;
//     }

//     get Any() {
//         return this.SelectedCount > 0;
//     }
//     get Single() {
//         return this.SelectedCount === 1;
//     }

//     get First() {
//         return this.SelectedCount > 0 ? this.SelectedData[0] : null;
//     }

//     constructor(gridApi: GridApi, private mEvent: MouseEvent) {
//         this.SelectedData = gridApi.getSelectedNodes().map(n => n.data)
//     }

//     get IsShiftPressed() {
//         return this.mEvent.shiftKey;
//     }

// }