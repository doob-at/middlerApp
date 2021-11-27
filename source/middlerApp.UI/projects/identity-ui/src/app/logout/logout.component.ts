import { Component, ChangeDetectionStrategy } from "@angular/core";
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs/operators';
import { Location } from '@angular/common';
import { IdpService } from "../idp-service";
import { LogOutModel } from "../models/log-out-model";

@Component({
    templateUrl: './logout.component.html',
    styleUrls: ['./logout.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LogoutComponent {

    viewModel = null;
    errors: any;

    logOutModel?: LogOutModel;

    constructor(private idpService: IdpService, private route: ActivatedRoute, private location: Location, private router: Router) { }

    ngOnInit(): void {
        
        this.idpService.GetLogoutViewModel(window.location.search)
        .subscribe(model => {
            this.logOutModel = model;
        });
        // this.route.queryParamMap.pipe(take(1)).subscribe(qmap => {

        //     let logoutid: string = "1";
        //     for (const key of qmap.keys) {
        //         if (key.toLowerCase() === 'logoutid') {
        //             logoutid = qmap.get(key) ?? "1";
        //         }
        //     }

            
        // });

    }

    public Cancel() {
        console.log("Cancel")
        if(this.logOutModel?.PostLogoutRedirectUri){
            window.location.href = this.logOutModel.PostLogoutRedirectUri;
        } else {
            this.location.back();
        }
        
    }

    public Logout() {
        this.idpService.CompleteLogOut().subscribe(_ => this.router.navigate(["/logged-out"]));
    }

}