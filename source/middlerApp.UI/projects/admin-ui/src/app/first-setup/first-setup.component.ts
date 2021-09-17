import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AppUIService } from '@shared/services';
import { FirstSetupService } from './first-setup.service';
import { FirstSetupModel } from './models/first-setup-model';

@Component({
    templateUrl: './first-setup.component.html',
    styleUrls: ['./first-setup.component.scss'],
    changeDetection: ChangeDetectionStrategy.Default
})
export class FirstSetupComponent {

    form!: FormGroup;
    errors = [];
    
    constructor(private uiService: AppUIService, private setupApi: FirstSetupService, private router: Router, private fb: FormBuilder, private cref: ChangeDetectorRef) {

        console.log("constructor")

        uiService.Set(ui => {
            ui.Header.Title = "First Setup"
            ui.Content.Scrollable = false;
            ui.Sidebar.Hide = true;
            //ui.Header.Icon = "fa#stream"
        })
        this.setupApi.GetSetupViewModel().subscribe(model => {
            console.log(model)
            if (model.AdminUserExists) {
                this.router.navigate(['/']);
            } else {
                this.createForm();
            }

        });


    }


    createForm() {

        this.form = this.fb.group({
            Username: ['', [Validators.required, Validators.minLength(5)]],
            Password: ['', [Validators.required, Validators.minLength(8)]],
            ConfirmPassword: ['', [Validators.required, Validators.minLength(8)]]
        });

        console.log(this.form)
        this.cref.detectChanges();

    }

 
    Submit() {

        if (!this.form.valid) {
            return;
        }

        let dto = <FirstSetupModel>this.form.value;
        this.errors = [];

        dto.RedirectUri = location.origin;
        this.setupApi.SubmitFirstSetupViewModel(dto).subscribe(okResp => {
            this.router.navigate(['/']);
        }, errorResp => {
            this.errors = errorResp.error;
        });
    }
}