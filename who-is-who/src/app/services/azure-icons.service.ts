import { Injectable } from '@angular/core';
import { LoggerService } from './logger.service';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AzureIconService {
  constructor(private http: HttpClient) { }

  public getSvg(name: string)
  {
    return this.http.get("/assets/azure-icons/" + name + ".svg", {responseType: 'text'});
  }  
}