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
    //var file = resourceMapping[name];
    var file = resourceMapping['User'];
    return this.http.get(resourceMapping[name], {responseType: 'text'});
  }
  
}

const resourceMapping: Record<string, string> = { 
  "User": "/assets/azure-icons/General%20Service%20Icons/User%20Icon.svg",
  "ResourceGroup": "/assets/azure-icons/General%20Service%20Icons/Resource%20Groups.svg",
  "Group": "/assets/azure-icons/_Flat%20Symbols/CnE_Intune/User%20group.svg",
  doorToDoor: "delivery at door",
  airDelivery: "flying in",
  specialDelivery: "special delivery",
  inStore: "in-store pickup",
};
