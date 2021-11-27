import { Component } from "@angular/core";
import { AppUIService } from "@shared/services";

@Component({
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {

    constructor(private uiService: AppUIService) {

        uiService.Set(ui => {
            ui.Header.Title = "Dashboard"
            ui.Header.Icon = "dashboard"
        });

    }
}