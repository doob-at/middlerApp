import { Component, OnInit, ChangeDetectorRef } from "@angular/core";
import { AppUIService } from "@shared/services";
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { combineLatest, from, Observable, of } from 'rxjs';
import { map, mergeAll, tap } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';

import { MRoleDto } from '../models/m-role-dto';
import { RolesService } from "./roles.service";

@Component({
    templateUrl: './role-details.component.html',
    styleUrls: ['./role-details.component.scss']
})
export class RoleDetailsComponent implements OnInit {


    form!: FormGroup


    private Id: string = '';
    public user$ = combineLatest([this.route.paramMap, this.route.queryParamMap, this.route.fragment]).pipe(
        map(([paramMap, queryParamMap, fragment]) => {
            this.Id = paramMap.get('id')!
            return this.rolesService.GetRole(this.Id);
        }),
        mergeAll(),
        tap(dto => {
            console.log(dto)
            this.setRole(dto)
        })
    )

    showDebugInformations$ = this.uiService.showDebugInformations$;

    constructor(
        private uiService: AppUIService,
        private fb: FormBuilder,
        private route: ActivatedRoute,
        private rolesService: RolesService,
        private cref: ChangeDetectorRef
    ) {
        uiService.Set(ui => {
            ui.Header.Title = "Role"
            ui.Content.Scrollable = false;
            ui.Header.Icon = "fa#user-tag"

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

            Name: [null, Validators.required],
            DisplayName: [null, Validators.required],
            Description: [null],
            Users: [[]]
        })

    }

    onOk(date: Date) {
        date.setHours(23, 59, 0, 0)
    }


    Save() {

        console.log(this);
        if (this.Id == 'create') {
            this.rolesService.CreateRole(this.form.value).subscribe();
        } else {
            this.rolesService.UpdateRole(this.form.value).subscribe();
        }
    }

    setRole(dto: MRoleDto) {
        if (!this.BaseDto) {
            this.BaseDto = dto;
        }

        this.uiService.Set(ui => {
            ui.Header.Title = "Role";
            //ui.Header.SubTitle = user.UserName
            ui.Header.Icon = "form"

            ui.Footer.Button1.Visible = true;
            ui.Footer.Button1.Text = dto.Id ? "Save Changes" : "Create Role"
            ui.Footer.Button2.Visible = true;
        })

        this.form.patchValue(dto)
    }

    Reload() {
        this.rolesService.GetRole(this.Id).subscribe(role => {
            this.setRole(role)
            this.rolesService.ReLoadRoles();
        });

        
    }

    private _baseDto!: MRoleDto;
    private set BaseDto(rule: MRoleDto) {
        this._baseDto = JSON.parse(JSON.stringify(rule));

    }
    private get BaseDto() {
        return this._baseDto;
    }
}