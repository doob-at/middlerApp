import { ChangeDetectionStrategy, Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { ActivatedRoute } from "@angular/router";
import { AppUIService } from "@shared/services";
import { combineLatest } from "rxjs";
import { map, mergeAll, tap } from "rxjs/operators";
import { ClientsService } from "./clients.service";
import { MClientDto } from "./models/MClientDto";

@Component({
    templateUrl: './client-details.component.html',
    styleUrls: ['./client-details.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClientDetailsComponent implements OnInit {


    form!: FormGroup

    private Id!: string;
    public client$ = combineLatest([this.route.paramMap, this.route.queryParamMap, this.route.fragment]).pipe(
        map(([paramMap, queryParamMap, fragment]) => {
            this.Id = paramMap.get('id')!
            return this.clientsService.GetClient(this.Id);
        }),
        mergeAll(),
        tap(client => this.setClient(client))
    )

    showDebugInformations$ = this.uiService.showDebugInformations$;

    constructor(
        private uiService: AppUIService,
        private fb: FormBuilder,
        private route: ActivatedRoute,
        private clientsService: ClientsService,
    ) {
        uiService.Set(ui => {
            ui.Header.Title = "Client"
            ui.Content.Scrollable = false;
            ui.Header.Icon = "fa#user"

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
            ClientId: [null],
            DisplayName: [null],
            Description: [null],

            RequirePkce: [false],

            RedirectUris: [[]],
            PostLogoutRedirectUris: [[]]
        })

    }

    Save() {

        console.log(this);
        if (this.Id == 'create') {
            this.clientsService.CreateClient(this.form.value).subscribe();
        } else {
            this.clientsService.UpdateClient(this.form.value).subscribe();
        }
    }

    setClient(dto: MClientDto) {
        if (!this.BaseDto) {
            this.BaseDto = dto;
        }

        this.uiService.Set(ui => {
            ui.Header.Title = "Client";
            //ui.Header.SubTitle = user.UserName
            ui.Header.Icon = "form"

            ui.Footer.Button1.Visible = true;
            ui.Footer.Button1.Text = dto.Id ? "Save Changes" : "Create Client"
            ui.Footer.Button2.Visible = true;
        })

        this.form.patchValue(dto)
    }

    Reload() {
        this.clientsService.GetClient(this.Id).subscribe(client => {
            this.setClient(client)
        });


    }

    private _baseDto!: MClientDto;
    private set BaseDto(rule: MClientDto) {
        this._baseDto = JSON.parse(JSON.stringify(rule));

    }
    private get BaseDto() {
        return this._baseDto;
    }

}