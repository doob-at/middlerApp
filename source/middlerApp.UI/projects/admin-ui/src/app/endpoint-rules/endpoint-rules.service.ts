import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { EndpointRule } from './models/endpoint-rule';
import { CreateMiddlerRuleDto } from './models/create-endpoint-rule';
import { of, BehaviorSubject } from 'rxjs';
import { tap, shareReplay, take, map } from 'rxjs/operators';
import { compare } from 'fast-json-patch';
import { DoobEditorFile } from '@doob-ng/editor';
import { EndpointRuleListDto } from './models/endpoint-rule-list-dto';
import { EndpointAction } from './models/endpoint-action';

import { EndpointRulesQuery, EndpointRulesStore } from './endpoint-rules-store';
import { MessageService } from "@shared/services";
import { DataEvent } from "@shared/models/data-event";




@Injectable({
    providedIn: 'root'
})
export class EndpointRulesService {

    private RulesOrder: {
        [key: string]: number
    } = {}

    constructor(
        private http: HttpClient,
        private message: MessageService,
        private endpointRulesStore: EndpointRulesStore,
        private endpointRulesQuery: EndpointRulesQuery

    ) {

        this.Subscribe();

        this.GetAllRules().subscribe()

        this.message.RunOnEveryReconnect(() => this.GetAllRules().subscribe());
        this.message.RunOnEveryReconnect(() => this.GetTypings());
        this.message.RunOnEveryReconnect(() => this.GetImports());

    }

    getHeaders(includePatchHeaders: boolean = false) {
        if (includePatchHeaders) {
            return new HttpHeaders({
                //'Authorization': 'Bearer ' + this.auth.getAccessToken(),
                'Content-Type': 'application/json-patch+json',
                'Accept': 'application/json'
            });
        } else {
            return new HttpHeaders({
                //'Authorization': 'Bearer ' + this.auth.getAccessToken()
            });
        }

    }

    Subscribe() {

        this.message.Stream<DataEvent<any>>("EndpointRules.Subscribe").pipe(
            tap(item => {
                switch (item.Action) {
                    case "Created": {
                        this.endpointRulesStore.add(item.Payload);
                        break;
                    }
                    case "Updated": {
                        if (item.MetaData.UpdateOrder) {
                            var orders = <{ [id: string]: number }>item.Payload
                            Object.keys(orders).forEach(id => {
                                this.endpointRulesStore.update(id, {
                                    Order: orders[id]
                                });
                            })
                        } else {
                            this.endpointRulesStore.update(item.Payload.Id, entity => item.Payload);
                        }

                        break;
                    }
                    case "Deleted": {
                        this.endpointRulesStore.update(<any>item.Payload, entity => ({
                            ...entity,
                            Deleted: true
                        }));
                        this.endpointRulesStore.remove(<any>item.Payload)
                    }
                }

            })
        ).subscribe()
    }

    ReLoadRules() {
        this.http.get<Array<EndpointRuleListDto>>(`/_api/endpoint-rules`, { headers: this.getHeaders() }).pipe(
            tap(rules => this.endpointRulesStore.set(rules))
        ).subscribe();
    }
    GetAllRules(force?: boolean) {
        if (force || !this.endpointRulesQuery.getHasCache()) {
            this.ReLoadRules()
        }

        return this.endpointRulesQuery.selectAll().pipe(
            map(rules => rules.sort(this.SortRules))
        );
    }


    public GetRule(id: string) {
        return this.http.get<EndpointRule>(`/_api/endpoint-rules/${id}`, { headers: this.getHeaders() });
    }

    public Add(rule: CreateMiddlerRuleDto) {
        return this.http.post(`/_api/endpoint-rules`, rule, { headers: this.getHeaders() });
    }


    public Remove(id: string) {
        return this.http.delete(`/_api/endpoint-rules/${id}`, { headers: this.getHeaders() });
    }

    public Update(id: string, rule: EndpointRule) {
        return this.http.put<EndpointRule>(`/_api/endpoint-rules/${id}`, rule, { headers: this.getHeaders() });
    }

    public UpdateAction(action: EndpointAction) {
        return this.http.put<EndpointAction>(`/_api/endpoint-action/${action.Id}`, action, { headers: this.getHeaders() });
    }

    public SortRules(a: EndpointRuleListDto, b: EndpointRuleListDto) {
        if (a.Order > b.Order) {
            return 1;
        } else if (a.Order < b.Order) {
            return -1;
        }
        return 0;
    }

    public GetNextLastOrder() {
        const rules = this.endpointRulesQuery.getAll();
        if (!rules || rules.length === 0) {
            return 10;
        }
        return Math.trunc(Math.max(...rules.map(r => r.Order)) + 10);
    }

   
    public UpdateRuleOrder(rule: EndpointRuleListDto, newOrder: number) {
        var rules:any = {};
        rules[rule.Id] = newOrder;
        this.http.post<EndpointRule>(`/_api/endpoint-rules/order`, rules, { headers: this.getHeaders(true) }).subscribe()
    }


    public SetRuleEnabled(rule: EndpointRuleListDto, value: boolean) {

        this.http.put<EndpointRule>(`/_api/endpoint-rules/${rule.Id}/enabled/${value}`, null, { headers: this.getHeaders(true) }).subscribe();

    }

    typings$: BehaviorSubject<DoobEditorFile[]> = new BehaviorSubject<DoobEditorFile[]>([]);

    public GetTypings() {
        this.http.get<Array<{ Key: string, Value: string }>>("/_api/endpoint-rules/type-definitions", { headers: this.getHeaders() })
            .pipe(
                take(1),
                tap(typings => {
                    var ts = typings.map(t => new DoobEditorFile(t.Key, t.Value, false))
                    this.typings$.next(ts);
                })
            ).subscribe();
    }

    imports$: BehaviorSubject<DoobEditorFile[]> = new BehaviorSubject<DoobEditorFile[]>([]);

    public GetImports() {
        this.http.get<Array<{ Key: string, Value: string }>>("/_api/endpoint-rules/import-definitions", { headers: this.getHeaders() })
            .pipe(
                take(1),
                tap(typings => {
                    var ts = typings.map(t => new DoobEditorFile(t.Key, t.Value, false))
                    this.imports$.next(ts);
                })
            ).subscribe();
    }

}

