import { Injectable } from '@angular/core';
import { LoggerService } from './logger.service';

import { HttpClient, HttpHeaders } from '@angular/common/http';
import { suggester_response } from '../model/suggester';
import { results_response } from '../model/results';
import { AppConfig } from '../app.config';

@Injectable({
  providedIn: 'root'
})

export class AzureSearchService {
  constructor( private logger: LoggerService, private http: HttpClient, private appConfig: AppConfig) { }

  Suggestions(text:string)
  {
    var get_url = this.appConfig.settings.ApiUrlBase + '/api/Suggest?s-earch=' + encodeURI(text);
    
    return this.http.get<suggester_response>(get_url, {
        observe: 'body',
        responseType: 'json',
      }); 
  }

  ResultsByText (text:string)
  {
    var get_url = this.appConfig.settings.ApiUrlBase + '/api/ResultByText?s-earch=' + encodeURI(text);

    return this.http.get<results_response>(get_url, {
      observe: 'body',
      responseType: 'json',
    });
  }

  ResultsByFilters (filters:string)
  {
    var get_url = this.appConfig.settings.ApiUrlBase + '/api/ResultByFilter?$filter=' + encodeURI(filters);

    return this.http.get<results_response>(get_url, {
      observe: 'body',
      responseType: 'json',
    });
  }
  
  ResultByKey (key:string)
  {
    var get_url = this.appConfig.settings.ApiUrlBase + "/api/ResultByFilter?&$filter=Key eq '" + encodeURI(key) + "'";

    return this.http.get<results_response>(get_url, {
      observe: 'body',
      responseType: 'json',
    });
  }

  IndexSize ()
  {
    var get_url = this.appConfig.settings.ApiUrlBase + "/api/Count";

    return this.http.get<string>(get_url, {
      observe: 'body',
      responseType: 'json',
    });
  }

}
