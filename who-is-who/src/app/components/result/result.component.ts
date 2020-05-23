import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeHtml} from '@angular/platform-browser';

import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { AzureSearchService } from '../../services/azure-search.service';
import { AzureIconService } from '../../services/azure-icons.service';

@Component({
  selector: 'app-result',
  templateUrl: './result.component.html',
  styleUrls: ['../../stylesCommon.css', './result.component.css']
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
  rowKey:string;
  errorMessage: string;
  resourceType: string;
  iconSVG: SafeHtml;
  name:string;
  surname:string;
  email:string;
  department:string;
  lastupdate:string;
  deeplink:string;

  public goDeeplink(url:string)
  {
    var win = window.open(url, '_blank');
    win.focus();
  }

  private populate()
  {
    var caller = this;
    
    this.SearchService.ResultByKey(caller.key).subscribe( {
      next(sr) {
        sr.value.forEach(element => {
          
          caller.resourceType = element.Type;
          caller.IconService.getSvg(element.Type).subscribe( data =>
            {
              data = data.replace("<svg ", "<svg style='width: 32; height: 32;' ")
              var sanit = caller.sanitizer.bypassSecurityTrustHtml(data);
              
              caller.iconSVG = sanit;
            });
            caller.name = element.Name;
            caller.surname = element.Surname;
            caller.email = element.Mail;
            caller.department = element.Department;
            caller.lastupdate = element.Timestamp.toString();
            caller.deeplink = element.DeepLink;
            caller.rowKey = element.RowKey;
        });    
      },
      error(msg) {
        caller.errorMessage = msg.message;
        console.log('error: ', msg.message);
      }
    });
  }
}
