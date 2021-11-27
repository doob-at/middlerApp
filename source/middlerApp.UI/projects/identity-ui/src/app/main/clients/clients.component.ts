import { ChangeDetectionStrategy, Component, HostListener, OnInit, TemplateRef, ViewChild, ViewContainerRef } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { DoobOverlayService, IOverlayHandle } from "@doob-ng/cdk-helper";
import { DefaultContextMenuContext, GridBuilder } from "@doob-ng/grid";
import { AppUIService } from "@shared/services";
import { ClientsService } from "./clients.service";


import { ClientsQuery } from "./clients.store";

@Component({
    templateUrl: './clients.component.html',
    styleUrls: ['./clients.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClientsComponent implements OnInit {

    @ViewChild('itemsContextMenu') itemsContextMenu!: TemplateRef<any>




    grid = new GridBuilder<any>()
        .SetColumns(
            c => c.Default("ClientId")
                .SetInitialWidth(200, true).SetLeftFixed()
                .SetCssClass("pValue"),
            c => c.Default("DisplayName")
                .SetInitialWidth(240, true),
            c => c.Default("Description")
        )
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
            let vContext = new DefaultContextMenuContext(api, ev)
            this.contextMenu = this.overlay.OpenContextMenu(ev, this.itemsContextMenu, this.viewContainerRef, vContext)
        })
        .StopEditingWhenCellsLoseFocus()
        .OnGridSizeChange(ev => ev.api.sizeColumnsToFit())
        .OnViewPortClick((ev, api) => {
            api.deselectAll();
        })
        .WithRowSelection("multiple")
        .WithFullRowEditType()
        .WithShiftResizeMode()
        .OnRowDoubleClicked(el => {
            this.EditClient(el.node.data);
        })
        .SetData(this.adminquery.selectAll())
        .SetDataImmutable(data => data.ClientId)

    private contextMenu!: IOverlayHandle;

    constructor(
        private appui: AppUIService,
        private router: Router,
        private route: ActivatedRoute,
        private adminapi: ClientsService,
        private adminquery: ClientsQuery,
        public overlay: DoobOverlayService,
        public viewContainerRef: ViewContainerRef) {

        appui.Set(c => {
            c.Header.Title = "Clients"
            c.Header.Icon = "fa#fas|desktop"
            c.Content.Scrollable = false
            //c.Content.Container = false
        })
    }

    ngOnInit() {

        this.adminapi.GetClients()
    }

    AddClient() {
        this.router.navigate(["create"], { relativeTo: this.route })
    }

    EditClient(role: any) {
        this.router.navigate([role.Id], { relativeTo: this.route })
    }

    RemoveClient(clients: Array<any>) {
        // this.idService.DeleteClient(...clients.map(c => c.Id)).subscribe();
        // this.contextMenu?.Close();
    }

    ReloadClientsList() {
        this.adminapi.GetClients()
    }
}