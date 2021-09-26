import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { DomSanitizer, SafeHtml} from '@angular/platform-browser';

import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { AzureSearchService } from '../../services/azure-search.service';
import { AzureIconService } from '../../services/azure-icons.service';

@Component({
  selector: 'app-result',
  templateUrl: './result.component.html',
  styleUrls: ['../../stylesCommon.css', './result.component.css'],
})
export class ResultComponent implements OnInit {

  constructor(private SearchService:AzureSearchService,
    private IconService:AzureIconService,
    private route: ActivatedRoute,
    private router: Router,
    private sanitizer:DomSanitizer,
    ) { }

  ngOnInit(): void {
      this.key = this.route.snapshot.paramMap.get('key');
      this.populate();
    }

  key: string;
  partitionKey:string;
  rowKey:string;

  type:string;
  
  errorMessage: string;
  resourceType: string;
  iconSVG: SafeHtml;
  name:string;
  surname:string;
  email:string;
  department:string;
  lastupdate:string;
  deeplink:string;
  ImgUrl:string;
  userType:string;
  groupType:string;
  typeIdColumn:string;
  IsWait:boolean ;
  
  public goDeeplink(url:string)
  {
    var win = window.open(url, '_blank');
    win.focus();
  }

  private populate()
  {
    var caller = this;
    caller.IsWait=true;
    this.SearchService.ResultByKey(caller.key).subscribe( {
      next(sr) {
        sr.value.forEach(element => {
          
          caller.resourceType = element.Type;
          caller.IconService.getSvg(element.Type).subscribe( data =>
            {
              data = data.replace("<svg ", "<svg style='width: 32px; height: 32px;' ")
              var sanit = caller.sanitizer.bypassSecurityTrustHtml(data);
              
              caller.iconSVG = sanit;
            });
            caller.type = element.Type;
            caller.name = element.Name;
            caller.surname = element.Surname;
            caller.email = element.Mail;
            caller.department = element.Department;
            caller.lastupdate = element.Timestamp.toString();
            caller.deeplink = element.DeepLink;
            caller.partitionKey = element.PartitionKey;
            caller.rowKey = element.RowKey;
            caller.ImgUrl = element.ImgUrl;
            caller.userType = element.UserType;
            caller.groupType = element.GroupType;
            caller.typeIdColumn = element.Type + "Id"; 
           
        });    
        caller.IsWait=false;
      },
      error(msg) {
        caller.errorMessage = msg.message;
        console.log('error: ', msg.message);
       
      }
    });
  }

  public hasValue (item:string)
  {
    return item != null;
  }  
}
