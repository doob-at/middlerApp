import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

@Injectable()
export class AdminUIConfigService {

    private readonly CONFIGURATION_URL = "/api/admin-ui/config";
    private _configuration!: AdminUIConfig;
    constructor(private _http: HttpClient) {

    }

    public loadConfiguration() {
        // return of(<AdminUIConfig>{IDPBaseUri: ""})
        // return this._http
        //     .get<AdminUIConfig>(this.CONFIGURATION_URL)
        //     .toPromise()
            return   of(<AdminUIConfig>{IDPBaseUri: "https://localhost"}).toPromise()
            .then((configuration: AdminUIConfig) => {
                this._configuration = configuration;
                console.log(configuration);
                return configuration;
            })
            .catch((error: any) => {
                console.error(error);
            });
    }

    getConfiguration() {
        return this._configuration;
    }

}

export interface AdminUIConfig {
    IDPBaseUri: string;
}
