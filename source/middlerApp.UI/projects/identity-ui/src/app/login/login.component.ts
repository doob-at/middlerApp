import { Component } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { take } from "rxjs/operators";
import { IdpService } from "../idp-service";
import { ExternalLoginModel } from "../models/external-login-model";
import { LoginInputModel } from "../models/login-input-model";
import { LoginViewModel } from "../models/login-view-model";

@Component({
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent {

    form!: FormGroup;

    viewModel!: LoginViewModel;
    errors: any;


    constructor(private fb: FormBuilder, private idpService: IdpService, private route: ActivatedRoute, private router: Router) {

     }

    ngOnInit(): void {
        this.form = this.fb.group({
            Username: [null, [Validators.required]],
            Password: [null, [Validators.required]],
            RememberLogin: [false]
        });

        this.route.queryParamMap.pipe(take(1)).subscribe(qmap => {

            let returnUrl: string | null;
            for (const key of qmap.keys) {
                if (key.toLowerCase() === 'returnurl') {
                    returnUrl = qmap.get(key);
                }
            }

            this.idpService.GetLoginViewModel(returnUrl!).subscribe((vm) => {
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
        model.ReturnUrl = this.viewModel.ReturnUrl;

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
                        console.log("OK")
                        
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
        model.ReturnUrl = this.viewModel.ReturnUrl;
        model.Scheme = scheme;
        
        this.idpService.SendExternalLoginInputModel(model).subscribe(result => {
            console.log(result);
            switch (result.Status) {
                case 'Confirmed':
                case 'Ok':
                    console.log("OK")
                    
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