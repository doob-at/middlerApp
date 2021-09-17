import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";


@Injectable({
    providedIn: 'root'
})
export class TestService {

    constructor(
        private http: HttpClient) {

    }

    public GetTest() {
                return this.http.get(`/_test`);
    }

}