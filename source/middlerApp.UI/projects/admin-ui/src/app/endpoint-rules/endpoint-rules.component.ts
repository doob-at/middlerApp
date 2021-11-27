import { ChangeDetectionStrategy, Component, TemplateRef, ViewChild, ViewContainerRef } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { DoobOverlayService, IOverlayHandle } from "@doob-ng/cdk-helper";
import { DefaultContextMenuContext, GridBuilder } from "@doob-ng/grid";
import { AppUIService } from "@shared/services";

import { EndpointRulesQuery } from "./endpoint-rules-store";
import { EndpointRulesService } from "./endpoint-rules.service";
import { EndpointRule } from "./models/endpoint-rule";
import { EndpointRuleListDto } from "./models/endpoint-rule-list-dto";

@Component({
    templateUrl: './endpoint-rules.component.html',
    styleUrls: ['./endpoint-rules.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class EndpointRulesComponent {


    @ViewChild('itemsContextMenu') itemsContextMenu!: TemplateRef<any>

    grid = new GridBuilder<EndpointRuleListDto>()
        .SetColumns(
            c => c.Default("Name")
                .SetInitialWidth(300, true)
                .Set(col => col.rowDrag = true)
                .Sortable(false),
            c => c.Default("Path").Sortable(false),
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
        .OnRowDoubleClicked(el => {
            this.EditRule(el.node.data.Id);
            //console.log("double Clicked", el)

        })
        .StopEditingWhenCellsLoseFocus()
        .OnGridSizeChange(ev => ev.api.sizeColumnsToFit())
        .OnViewPortClick((ev, api) => {
            api.deselectAll();
        })
        .WithRowSelection("multiple")
        .WithFullRowEditType()
        .WithShiftResizeMode()
        .SetData(this.endpointRulesQuery.selectAll())
        .SetDataImmutable(d => d.Id)

    private contextMenu!: IOverlayHandle;

    constructor(
        private appui: AppUIService,
        private endpointRulesQuery: EndpointRulesQuery,
        private rulesService: EndpointRulesService,
        public overlay: DoobOverlayService,
        public viewContainerRef: ViewContainerRef,
        private router: Router,
        private route: ActivatedRoute) {

        appui.Set(c => {
            c.Header.Title = "Endpoint Rules"
            c.Header.Icon = "fa#fas|stream"
            c.Content.Scrollable = false
        })
    }

    SetRuleEnabled(rule: EndpointRuleListDto, value: boolean) {
        this.contextMenu?.Close();
        this.rulesService.SetRuleEnabled(rule, value)

    }

    CreateRuleOnTop() {
        let rule = new EndpointRule();
        rule.Enabled = false;
        if (this.endpointRulesQuery.getCount() == 0) {
            rule.Order = 10;
        } else {
            rule.Order = this.endpointRulesQuery.getAll()[0].Order / 2;
        }
        this.rulesService.Add(rule).subscribe();
        this.contextMenu.Close();
    }

    CreateRuleBefore(item: any) {

        // const ruleindex = this.rules.findIndex(r => r.Order === item.Order)

        // let rule = new EndpointRule();
        // rule.Enabled = false;

        // if (ruleindex == 0) {
        //     rule.Order = item.Order / 2;
        // } else {
        //     rule.Order = item.Order - ((item.Order - this.rules[ruleindex - 1].Order) / 2)
        // }

        // console.log(rule);
        // this.rulesService.Add(rule).subscribe();

        // this.ContextMenu.Close();
    }

    CreateRuleAfter(item: any) {

        // const ruleindex = this.rules.findIndex(r => r.Order === item.Order)

        // let rule = new EndpointRule();
        // rule.Enabled = false;

        // if (ruleindex == this.rules.length - 1) {
        //     rule.Order = this.rulesService.GetNextLastOrder();
        // } else {
        //     rule.Order = item.Order + ((this.rules[ruleindex + 1].Order - item.Order) / 2)
        // }

        // console.log(rule);
        // this.rulesService.Add(rule).subscribe();

        // this.ContextMenu.Close();
    }

    CreateRuleOnBottom() {
        //     let rule = new EndpointRule();
        //     rule.Enabled = false;
        //     rule.Order = this.rulesService.GetNextLastOrder();
        //     this.rulesService.Add(rule).subscribe();
        //     this.ContextMenu.Close();
    }

    RemoveRules(rules: Array<any>) {
        rules.forEach(act => this.rulesService.Remove(act.Id).subscribe());
        this.contextMenu.Close()
    }

    EditRule(id: string) {
        this.router.navigate([id], { relativeTo: this.route })
    }

}