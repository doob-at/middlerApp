import { Component, ChangeDetectionStrategy } from "@angular/core";
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs/operators';
import { Location } from '@angular/common';
import { IdpService } from "../idp-service";
import { LogOutModel } from "../models/log-out-model";

@Component({
    templateUrl: './logged-out.component.html',
    styleUrls: ['./logged-out.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoggedOutComponent {

    logOutModel!: LogOutModel;
   

    constructor(private idpService: IdpService) { }

    ngOnInit() {

        console.log(this.idpService.logOutModel);
        
        // if (!this.idpService.logOutModel) {
        //     this.location.back();
        //     return;
        // }

        // if (!this.idpService.logOutModel.ClientName) {
        //     window.location.href = '/';
        // }
        // if (this.idpService.logOutModel.AutomaticRedirectAfterSignOut) {
        //     window.location.href = this.idpService.logOutModel.PostLogoutRedirectUri;
        //     return;
        // }

        this.logOutModel = this.idpService.logOutModel;
    }

}