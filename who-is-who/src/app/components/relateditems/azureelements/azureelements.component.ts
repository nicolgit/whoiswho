import { Component, OnInit, Input } from '@angular/core';
import { DomSanitizer, SafeHtml} from '@angular/platform-browser';

import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { AzureSearchService } from '../../../services/azure-search.service';
import { AzureIconService } from '../../../services/azure-icons.service';

export class Result {
  iconSVG: SafeHtml;
  
  row1: string;
  row2: string;
  
  key: string;
}

@Component({
  selector: 'related-azure-elements',
  templateUrl: './azureelements.component.html',
  styleUrls: [ '../../../stylesCommon.css', './azureelements.component.css']
})
export class AzureElementsComponent implements OnInit {

  constructor(private SearchService:AzureSearchService,
    private IconService:AzureIconService,
    private route: ActivatedRoute,
    private router: Router,
    private sanitizer:DomSanitizer,
    ) { 
    }

  ngOnInit(): void {
  }

  @Input() Name: string;
  @Input() ParentKey:string;  

  @Input() FilterCorelationType: string;
  @Input() FilterElementType:string;
  @Input() FilterElementTypeId:string;
  
  private _parentValue;
  @Input() 
  set ParentValue (pv:string) {
    if (pv != null && pv != "" )
    {
      this._parentValue = pv;
      this.RefreshUI();
    }
  }
  get ParentValue():string {
    return this._parentValue;
  }

  errorMessage: string;
  iconSVG: SafeHtml;
  
  get hasData(): boolean {
    return this.searchResults != null && this.searchResults.length>0;
  }
  
  searchResults: Result[];

  public goDeeplink(url:string)
  {
    var win = window.open(url, '_blank');
    win.focus();
  }

  public RefreshUI()
  {
    var caller = this;
    caller.searchResults = [];
    
    var filters = caller.FilterCorelationType + " and " + caller.ParentKey + " eq '" + caller.ParentValue + "'";

    this.SearchService.ResultsByFilters(filters).subscribe( {
      next(sr) {
        sr.value.forEach(element => {
          var s = new Result();

          s.key = element.Key
          s.row1 = "?";
          s.row2 = element.Name;

          var id = "";

          switch (caller.FilterElementType) {
            case 'Subscription':
              id = element.SubscriptionId;
              break;
            case 'ResourceGroup':
              id = element.ResourceGroupId;
              break;
            case 'Resource':
              id = element.ResourceId;
              break;
            case 'Group':
              id = element.GroupId;
              break;
            case 'Application':
              id = element.ApplicationId;
              break;
            default:
              throw "not implemented yet! (" + caller.FilterElementType + "')";
          }

          var filters2 = "Type eq '" + caller.FilterElementType + "' and RowKey eq '" + id + "'";

          caller.SearchService.ResultsByFilters(filters2).subscribe( { 
            next(sr) {             
              sr.value.forEach(element => {
                s.row1 = element.Name
                
                caller.IconService.getSvg(element.Type).subscribe( data =>
                  {
                    data = data.replace("<svg ", "<svg style='width: 32; height: 32;' ")
                    var sanit = caller.sanitizer.bypassSecurityTrustHtml(data);
                    
                    s.iconSVG = sanit;
                  }
                ); 
              });
            },
            error(msg) {
              caller.errorMessage = msg.message;
              console.log('error ResultByKey: ', msg.message);
            }
          });

          caller.searchResults.push(s);
        });
      },
      error(msg) {
        caller.errorMessage = msg.message;
        console.log('error: ', msg.message);
      }
    });
  }
}