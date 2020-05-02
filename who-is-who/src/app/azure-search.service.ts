import { Injectable } from '@angular/core';
import { LoggerService } from './logger.service';

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators'

import { suggester_response, Value } from './model/suggester';
@Injectable({
  providedIn: 'root'
})

export class AzureSearchService {

  private query_key = 'EE7A8787FBAD8D2A6C44140947211933';
  private base_url = 'https://whoiswho-engine.search.windows.net';
  private index = 'azuretable-index';

  private suggestions_url = this.base_url + '/indexes/' + this.index + '/docs/suggest?api-version=2019-05-06';

  constructor( private logger: LoggerService, private http: HttpClient) { }

  Sum (a:number, b:number)
  {
    return a + b;
  }

  Suggestions(text:string)
  {
    var get_url = this.suggestions_url + '&search=' + encodeURI(text) + '&suggesterName=default';

    return this.http.get<suggester_response>(get_url, {
        observe: 'body',
        responseType: 'json',
        headers: new HttpHeaders({
          'api-key': this.query_key
        }),
      });
  }
}
