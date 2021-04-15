import { HttpBackend, HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { IAppConfig } from './model/app-config.model';
import { Injectable } from '@angular/core';

@Injectable()
export class AppConfig {
    public settings: IAppConfig;
    private http: HttpClient;
    constructor(private readonly httpHandler: HttpBackend) {
        this.http = new HttpClient(httpHandler);
    }
    load() {
        const jsonFile = `assets/config/config.${environment.name}.json`;
        return new Promise<void>((resolve, reject) => {
            this.http.get(jsonFile).toPromise().then((response: IAppConfig) => {
                this.settings = <IAppConfig>response;
                resolve();
            }).catch((response: any) => {
                reject(`Could not load file '${jsonFile}': ${JSON.stringify(response)}`);
            });
        });
    }
}