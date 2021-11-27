import { ChangeDetectionStrategy, Component } from "@angular/core";
import { FormBuilder, FormGroup } from '@angular/forms';
import { OverlayContext } from '@doob-ng/cdk-helper';
import { BehaviorSubject } from 'rxjs';
import { PermissionEntry } from './permission-entry';


@Component({
    templateUrl: './add-rule-modal.component.html',
    styleUrls: ['./add-rule-modal.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddPermissionRuleComponent {

    form: FormGroup;
    
    constructor(private fb: FormBuilder, private context: OverlayContext<PermissionEntry>) {

        this.form = fb.group({
            Id: [],
            PrincipalName: [],
            Type: ["User"],
            Order: [],
            AccessMode: ["Allow"],
            BoundToClient: [],
            Client: [],
            BoundToSourceAddress: [],
            SourceAddress: []
        });
    }

    ngOnInit() {

        this.Get("BoundToClient")?.valueChanges.subscribe(value => {
            if (!!value) {
                this.Get("Client")?.enable();
            } else {
                this.Get("Client")?.disable();
            }

        });

        this.Get("BoundToSourceAddress")?.valueChanges.subscribe(value => {
            if (!!value) {
                this.Get("SourceAddress")?.enable();
            } else {
                this.Get("SourceAddress")?.disable();
            }

        });

        this.Get("Type")?.valueChanges.subscribe(val => {
            console.log("Type", val)
            if(val == "Authenticated" || val == "Everyone") {
                this.Get("PrincipalName")?.disable();
            } else {
                this.Get("PrincipalName")?.enable();
            }
        })

        this.form.patchValue(this.context.data);

        this.Get("BoundToClient")?.patchValue(this.Get("Client")?.value)
        this.Get("BoundToSourceAddress")?.patchValue(this.Get("SourceAddress")?.value)

    }

    ok() {
        this.context.invoke("OK", this.form.value);
    }

    cancel() {
        this.context.handle.Close();
    }

    Get(field: string) {
        return this.form.get(field);
    }
    IsEnabled(field: string) {
        var enabled = this.form.get(field)?.enabled ?? false;
        return enabled;
    }

    
}