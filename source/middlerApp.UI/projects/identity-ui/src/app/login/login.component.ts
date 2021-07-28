import { Component, ChangeDetectionStrategy } from "@angular/core";
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { IdpService } from '../shared/services/idp/idp.service';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs/operators';
import { LoginInputModel } from '../shared/services/idp/models/login-input-model';
import { AuthService } from '../shared/services';
import { ExternalLoginModel } from '../shared/services/idp/models/external-login-model';
import { LoginViewModel } from "../shared/services/idp/models/login-view-model";

@Component({
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IdpUILoginComponent {

    form!: FormGroup;

    viewModel: LoginViewModel = new LoginViewModel();
    errors: any;


    constructor(private fb: FormBuilder, private idpService: IdpService, private route: ActivatedRoute, private router: Router, private auth: AuthService) { }

    ngOnInit(): void {
        this.form = this.fb.group({
            Username: [null, [Validators.required]],
            Password: [null, [Validators.required]],
            RememberLogin: [false]
        });

        this.route.queryParamMap.pipe(take(1)).subscribe(qmap => {

            let returnUrl: string = "";
            for (const key of qmap.keys) {
                if (key.toLowerCase() === 'returnurl') {
                    returnUrl = qmap.get(key) ?? "";
                }
            }

            this.idpService.GetLoginViewModel(returnUrl).subscribe(vm => {
                this.viewModel = vm;
                this.form.patchValue(vm);
                // setTimeout(() => {
                //     this.form.get('Provider').patchValue(vm.DefaultProvider);
                // }, 0);

            });
        });
    }

    submitForm(): void {
        for (const i in this.form.controls) {
            this.form.controls[i].markAsDirty();
            this.form.controls[i].updateValueAndValidity();
        }

        if (!this.form.valid) {

            return;
        }

        const model = <LoginInputModel>this.form.value;
        model.ReturnUrl = this.viewModel?.ReturnUrl ?? "";

        console.log(this.form.value);

        this.idpService.SendLoginInputModel(model)
            .pipe(

            )
            .subscribe(result => {

                this.errors = result.Errors;
                console.log("result:", result)
                switch (result.Status) {
                    case 'Confirmed':
                    case 'Ok':
                        // console.log("OK")
                        // this.auth.GetAccessToken();
                        if (result.ReturnUrl) {
                            // const url =  window.location.origin + result.ReturnUrl;
                            // console.log("locations", url)
                            window.location.href = result.ReturnUrl;
                        } else {
                            window.location.href = "/"
                        }

                        break;
                    case 'MustConfirm':
                        this.router.navigate(['account', 'confirmation']);
                        break;
                    case 'Error':

                        break;
                }

            });
    }

    LoginExternal(scheme: string) {
        console.log(this.viewModel);

        const model = new ExternalLoginModel();
        model.ReturnUrl = this.viewModel?.ReturnUrl ?? "";
        model.Scheme = scheme;
        
        this.idpService.SendExternalLoginInputModel(model).subscribe(result => {
            console.log(result);
            switch (result.Status) {
                case 'Confirmed':
                case 'Ok':
                    // console.log("OK")
                    // this.auth.GetAccessToken();
                    if (result.ReturnUrl) {
                        // const url =  window.location.origin + result.ReturnUrl;
                        // console.log("locations", url)
                        window.location.href = result.ReturnUrl;
                    } else {
                        window.location.href = "/"
                    }

                    break;
                case 'MustConfirm':
                    this.router.navigate(['account', 'confirmation']);
                    break;
                case 'Error':

                    break;
            }
        })
    }

}