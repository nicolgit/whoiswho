import { Injectable } from '@angular/core';
import { LoggerService } from './logger.service';

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators'

import { suggester_response } from '../model/suggester';
import { results_response } from '../model/results';

import { AppConfig } from '../app.config';

@Injectable({
  providedIn: 'root'
})

export class AzureSearchService {

  private query_key = 'EE7A8787FBAD8D2A6C44140947211933'; 
  private base_url = 'https://whoiswho-engine.search.windows.net';
  private index = 'azuretable-index';

  constructor( private logger: LoggerService, private http: HttpClient, private appConfig: AppConfig) { }

  Suggestions(text:string)
  {
    var get_url = this.appConfig.settings.ApiUrlBase + '/api/Suggest?search=' + encodeURI(text);
    
    return this.http.get<suggester_response>(get_url, {
        observe: 'body',
        responseType: 'json',
      });
  }

  ResultsByText (text:string)
  {
    var get_url = this.appConfig.settings.ApiUrlBase + '/api/ResultByText?search=' + encodeURI(text);

    return this.http.get<results_response>(get_url, {
      observe: 'body',
      responseType: 'json',
      headers: new HttpHeaders({
        'api-key': this.query_key
      }),
    });
  }

  ResultsByFilters (filters:string)
  {
    var get_url = this.appConfig.settings.ApiUrlBase + '/api/ResultByFilter?$filter=' + encodeURI(filters);

    return this.http.get<results_response>(get_url, {
      observe: 'body',
      responseType: 'json',
      headers: new HttpHeaders({
        'api-key': this.query_key
      }),
    });
  }
  
  ResultByKey (key:string)
  {
    var get_url = this.appConfig.settings.ApiUrlBase + "/api/ResultByFilter?&$filter=Key eq '" + encodeURI(key) + "'";

    return this.http.get<results_response>(get_url, {
      observe: 'body',
      responseType: 'json',
      headers: new HttpHeaders({
        'api-key': this.query_key
      }),
    });
  }

  IndexSize ()
  {
    var get_url = this.appConfig.settings.ApiUrlBase + "/api/Count";

    return this.http.get<string>(get_url, {
      observe: 'body',
      responseType: 'json',
      headers: new HttpHeaders({
        'api-key': this.query_key
      }),
    });
  }

}
