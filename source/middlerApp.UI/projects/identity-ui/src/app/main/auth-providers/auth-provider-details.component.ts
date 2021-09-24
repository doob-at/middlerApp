import { Component, OnInit, ChangeDetectorRef } from "@angular/core";
import { AppUIService } from "@shared/services";
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { combineLatest, from, Observable, of } from 'rxjs';
import { map, mergeAll, tap } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';
import { AuthProviderService } from "./auth-providers.service";
import { AuthProviderDto } from "./models/auth-provider-dto";



@Component({
    templateUrl: './auth-provider-details.component.html',
    styleUrls: ['./auth-provider-details.component.scss']
})
export class AuthProviderDetailsComponent implements OnInit {


    form!: FormGroup


    private Id!: string;
    public provider$ = combineLatest(this.route.paramMap, this.route.queryParamMap, this.route.fragment).pipe(
        map(([paramMap, queryParamMap, fragment]) => {
            this.Id = paramMap.get('id')!
            return this.authProviderService.GetAuthProvider(this.Id);
        }),
        mergeAll(),
        tap(dto => this.setProvider(dto))
    )

    showDebugInformations$ = this.uiService.showDebugInformations$;

    constructor(
        private uiService: AppUIService,
        private fb: FormBuilder,
        private route: ActivatedRoute,
        private authProviderService: AuthProviderService,
        private cref: ChangeDetectorRef
    ) {
        uiService.Set(ui => {
            ui.Header.Title = "Authentication Provider"
            ui.Content.Scrollable = false;
            ui.Header.Icon = "fa#shield-alt"

            ui.Footer.Show = true;
            ui.Footer.Button1.Text = "Save";
            ui.Footer.Button2.Text = "Reset";

            ui.Footer.Button1.OnClick = () => this.Save();

            ui.Footer.Button2.OnClick = () => {
                this.form.reset(this.BaseDto);
            }
        })
    }

    ngOnInit() {

        this.form = this.fb.group({
            Id: [null],
            Type: [null],
            Enabled: [false],
            Name: [null, Validators.required],
            DisplayName: [null, Validators.required],
            Description: [null],
            Parameters: []
        })

    }


    Save() {

        console.log(this);
        if (this.Id == 'create') {
            this.authProviderService.CreateAuthProvider(this.form.value).subscribe();
        } else {
            this.authProviderService.UpdateAuthProvider(this.form.value).subscribe();
        }
    }

    setProvider(dto: AuthProviderDto) {
        if (!this.BaseDto) {
            this.BaseDto = dto;
        }

        this.uiService.Set(ui => {
            ui.Header.Title = "Authentication Provider";
            //ui.Header.SubTitle = user.UserName
            ui.Header.Icon = "form"

            ui.Footer.Button1.Visible = true;
            ui.Footer.Button1.Text = dto.Id ? "Save Changes" : "Create ApiScope"
            ui.Footer.Button2.Visible = true;
        })

        this.form.patchValue(dto)
    }

    Reload() {
        this.authProviderService.GetAuthProvider(this.Id).subscribe(apiresource => {
            this.setProvider(apiresource)
            this.authProviderService.ReLoadAuthProviders();
        });


    }

    private _baseDto!: AuthProviderDto;
    private set BaseDto(rule: AuthProviderDto) {
        this._baseDto = JSON.parse(JSON.stringify(rule));

    }
    private get BaseDto() {
        return this._baseDto;
    }
}