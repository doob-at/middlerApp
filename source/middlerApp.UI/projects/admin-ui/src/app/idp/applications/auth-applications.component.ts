import { ChangeDetectionStrategy, Component, HostListener, OnInit, TemplateRef, ViewChild, ViewContainerRef } from "@angular/core";
import { DoobOverlayService, IOverlayHandle } from "@doob-ng/cdk-helper";
import { DefaultContextMenuContext, GridBuilder } from "@doob-ng/grid";
import { AppUIService } from "@shared/services";
import { AuthApplicationService } from "./auth-application.service";


import { AuthApplicationQuery } from "./auth-application.store";

@Component({
    templateUrl: './auth-applications.component.html',
    styleUrls: ['./auth-applications.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AuthApplicationsComponent implements OnInit {

    @ViewChild('itemsContextMenu') itemsContextMenu!: TemplateRef<any>

   
    
    
    grid = new GridBuilder<any>()
        .SetColumns(
            c => c.Default("ClientId")
                .SetInitialWidth(300, true)
                .Set(col => col.rowDrag = true)
                .Sortable(false),
            c => c.Default("DisplayName").Sortable(false),
            c => c.Default("AccessMode").Sortable(false),
            c => c.Default("Client").Sortable(false),
            c => c.Default("SourceAddress").Sortable(false)
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
            let vContext = new DefaultContextMenuContext(api, ev)
            this.contextMenu = this.overlay.OpenContextMenu(ev, this.itemsContextMenu, this.viewContainerRef, vContext)
        })
        .StopEditingWhenGridLosesFocus()
        .OnGridSizeChange(ev => ev.api.sizeColumnsToFit())
        .OnViewPortClick((ev, api) => {
            api.deselectAll();
        })
        .WithRowSelection("multiple")
        .WithFullRowEditType()
        .WithShiftResizeMode()
        
        .SetData(this.adminquery.selectAll())
        .SetDataImmutable(data => data.ClientId)

    private contextMenu!: IOverlayHandle;

    constructor(
        private appui: AppUIService, 
        private adminapi: AuthApplicationService, 
        private adminquery: AuthApplicationQuery,
        public overlay: DoobOverlayService,
        public viewContainerRef: ViewContainerRef) {

        appui.Set(c => {
            c.Header.Title = "Applications"
            c.Header.Icon = "fa#fas|stream"
            c.Content.Scrollable = false
            //c.Content.Container = false
        })
    }

    ngOnInit() {

        this.adminapi.GetApplications()
    }

    AddClient() {
        // this.router.navigate(["create"], { relativeTo: this.route })
    }

    EditClient(role: any) {
        // this.router.navigate([role.Id], { relativeTo: this.route })
    }

    RemoveClient(clients: Array<any>) {
        // this.idService.DeleteClient(...clients.map(c => c.Id)).subscribe();
        // this.contextMenu?.Close();
    }

    ReloadClientsList() {
        this.adminapi.GetApplications()
    }
}