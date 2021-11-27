import { Component, TemplateRef, ViewChild, ViewContainerRef } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";
import { IOverlayHandle, DoobOverlayService } from "@doob-ng/cdk-helper";
import { GridBuilder, DefaultContextMenuContext } from "@doob-ng/grid";
import { AppUIService } from "@shared/services";
import { AuthProviderService } from "./auth-providers.service";
import { AuthProvidersQuery } from "./auth-providers.store";
import { AuthProviderDto } from "./models/auth-provider-dto";

@Component({
    templateUrl: './auth-providers.component.html',
    styleUrls: ['./auth-providers.component.scss']
})
export class AuthProvidersComponent {

    @ViewChild('itemsContextMenu') itemsContextMenu!: TemplateRef<any>

    grid = new GridBuilder<AuthProviderDto>()
        .SetColumns(
            c => c.Default("Name")
                .SetInitialWidth(200, true)
                .SetLeftFixed()
                .SetCssClass("pValue"),
            c => c.Default("DisplayName"),
            c => c.Default("Description")
        )
        .SetData(this.authProviderQuery.selectAll())
        .WithRowSelection("multiple")
        .WithFullRowEditType()
        .WithShiftResizeMode()
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
        .OnRowDoubleClicked(el => {
            this.Edit(el.node.data);
            //console.log("double Clicked", el)

        })
        .StopEditingWhenCellsLoseFocus()
        .OnGridSizeChange(ev => ev.api.sizeColumnsToFit())
        .OnViewPortClick((ev, api) => {
            api.deselectAll();
        })
        .SetRowClassRules({
            'deleted': 'data.Deleted'
        })
        .SetDataImmutable(data => data.Id);



    private contextMenu?: IOverlayHandle;

    constructor(
        private uiService: AppUIService,
        private authProviderService: AuthProviderService,
        private authProviderQuery: AuthProvidersQuery,
        private router: Router,
        private route: ActivatedRoute,
        public overlay: DoobOverlayService,
        public viewContainerRef: ViewContainerRef
    ) {
        uiService.Set(ui => {
            ui.Header.Title = "IDP / Authentication Providers"
            ui.Content.Scrollable = false;
            ui.Header.Icon = "fa#shield-alt"
        })

        // idService.GetAllApiScopes().subscribe(api-resources => {
        //     this.grid.SetData(api-resources);
        // });
    }

    Add() {
        this.router.navigate(["create"], { relativeTo: this.route });
        this.contextMenu?.Close();
    }

    Edit(item: AuthProviderDto) {
        this.router.navigate([item.Id], { relativeTo: this.route });
        this.contextMenu?.Close();
    }

    Remove(item: Array<AuthProviderDto>) {
        this.authProviderService.DeleteAuthProvider(...item.map(r => r.Id)).subscribe();
        this.contextMenu?.Close();
    }

    Reload() {
        this.authProviderService.ReLoadAuthProviders();
    }
}