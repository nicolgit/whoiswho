import { Injectable } from '@angular/core';
import { LoggerService } from './logger.service';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AzureIcon {
  constructor(private http: HttpClient) { }

  getAzureSvg(name: string)
  {
    
  }
  
}

const resourceMapping: Record<string, string> = { 
      
  doorToDoor: "delivery at door",
  airDelivery: "flying in",
  specialDelivery: "special delivery",
  inStore: "in-store pickup",
};
