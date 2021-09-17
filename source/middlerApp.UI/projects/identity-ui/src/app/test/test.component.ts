import { Component } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { TestService } from "./test.service";

@Component({
    template: "<pre>{{data$ | async | json}}</pre>"
})
export class TestComponent {

    data$ = new BehaviorSubject<any>({});

    constructor(private testService: TestService) {

    }

    ngOnInit() {
        this.testService.GetTest().subscribe(d => this.data$.next(d));
    }
}