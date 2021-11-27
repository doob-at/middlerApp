import { Component, Input, Output, EventEmitter, HostListener } from "@angular/core";
import { FormGroup } from '@angular/forms';

@Component({
    selector: 'action-basic-modal',
    templateUrl: './action-basic-modal.component.html',
    styleUrls: ['./action-basic-modal.component.scss', '../style.scss'],
    host: {
        class: 'flex-grow-column action-basic-modal'
    }
})
export class ActionBasicModalComponent {

    @Output() ok = new EventEmitter();
    @Output() save = new EventEmitter();
    @Output() cancel = new EventEmitter();

    @Input() form!: FormGroup;

    private ctrls = false;
    @HostListener('window:keydown', ['$event'])
    keydown($event: KeyboardEvent) {
        $event.stopPropagation();
        const charCode = String.fromCharCode($event.which).toLowerCase();
        if ($event.ctrlKey && charCode === 's') {
            $event.preventDefault();
            if (!this.ctrls) {
                this.ctrls = true;
                this.save.next();
            }
        }
    }
    @HostListener('window:keyup', ['$event'])
    keyup($event: KeyboardEvent) {
        $event.stopPropagation();
        const charCode = String.fromCharCode($event.which).toLowerCase();
        if ($event.ctrlKey && charCode === 's') {
            $event.preventDefault();
            this.ctrls = false;
        }
    }


}
