import { Injectable } from '@angular/core';
import { LoggerService } from './logger.service';

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators'

import { suggester_response } from '../model/suggester';
import { results_response } from '../model/results';

@Injectable({
  providedIn: 'root'
})

export class AzureSearchService {

  private query_key = 'EE7A8787FBAD8D2A6C44140947211933';
  private base_url = 'https://whoiswho-engine.search.windows.net';
  private index = 'azuretable-index';

  private suggestions_url = this.base_url + '/indexes/' + this.index + '/docs/suggest?api-version=2019-05-06&suggesterName=default&highlightPreTag=<b>&highlightPostTag=</b>&$select=Type,Key,Name&fuzzy=true';
  private results_url = this.base_url + '/indexes/' + this.index + '/docs?api-version=2019-05-06';

  constructor( private logger: LoggerService, private http: HttpClient) { }

  Sum (a:number, b:number)
  {
    return a + b;
  }

  Suggestions(text:string)
  {
    var get_url = this.suggestions_url + '&search=' + encodeURI(text);

    return this.http.get<suggester_response>(get_url, {
        observe: 'body',
        responseType: 'json',
        headers: new HttpHeaders({
          'api-key': this.query_key
        }),
      });
  }

  Results (text:string)
  {
    var get_url = this.results_url + '&search=' + encodeURI(text);

    return this.http.get<results_response>(get_url, {
      observe: 'body',
      responseType: 'json',
      headers: new HttpHeaders({
        'api-key': this.query_key
      }),
    });
  }
}
