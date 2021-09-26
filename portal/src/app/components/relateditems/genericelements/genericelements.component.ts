import { Component, OnInit, Input } from '@angular/core';
import { DomSanitizer, SafeHtml} from '@angular/platform-browser';

import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { AzureSearchService } from '../../../services/azure-search.service';
import { AzureIconService } from '../../../services/azure-icons.service';

export class Result {
  iconSVG: SafeHtml;
  
  row1: string;
  row2: string;
  row3: string;
  
  partition: string;
  key: string;
}

@Component({
  selector: 'related-generic-elements',
  templateUrl: './genericelements.component.html',
  styleUrls: [ '../../../stylesCommon.css', './genericelements.component.css']
})
export class GenericElementsComponent implements OnInit {

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
  @Input() CorelationType: string;
  @Input() KeyIs: string;
  
  private _partitionKey;
  @Input()
  set PartitionKey (pt:string) {
    this._partitionKey = pt;
    this.RefreshUI();
  }
  get PartitionKey():string {
    return this._partitionKey;
  }

  private _rowKey;
  @Input()
  set RowKey (pt:string) {
    this._rowKey = pt;
    this.RefreshUI();
  }
  get RowKey():string {
    return this._rowKey;
  }

  errorMessage: string;
  iconSVG: SafeHtml;
  IsWaitChild:boolean;
  get hasData(): boolean {
    
    return this.searchResults != null && this.searchResults.length>0;
  }

  searchResults: Result[];

  public goDeeplink(url:string) 
  {
    var win = window.open(url, '_blank');
    win.focus();
  }

  public go(url:string)
  {
    this.router.navigateByUrl('/result/' + encodeURI(url) ).then(() => {
      this.router.navigated = false;
      this.router.navigate([this.router.url]);
    }).then((e) => {
      window.location.reload();
    });
  }

  public RefreshUI()
  {
    var caller = this;
    caller.searchResults = [];
    caller.IsWaitChild =true;
    if (this.CorelationType==null || caller.RowKey==null || caller.PartitionKey==null)
      return;

    var filters = "";
    switch (caller.KeyIs)
    {
      case 'Child':
          filters ="Type eq '" + caller.CorelationType + "' and ChildRowKey eq '" + caller.RowKey + "' and ChildPartitionKey eq '" + caller.PartitionKey + "'";
        break;
      case 'Parent':
          filters ="Type eq '" + caller.CorelationType + "' and ParentRowKey eq '" + caller.RowKey + "' and ParentPartitionKey eq '" + caller.PartitionKey + "'";
        break;
      default:
        throw "ERROR - CorelationType not valid('" + caller.CorelationType + "')";
    }
    
    this.SearchService.ResultsByFilters(filters).subscribe( {
      next(sr) {
        sr.value.forEach(element => {
          var s = new Result();

          s.key = element.Key
          s.partition = element.PartitionKey
          s.row1 = "?";
          s.row2 = element.Name;
          s.row3 = "";

          var id = "";
          
          var filters2 = "";
          switch (caller.KeyIs)
          {
            case 'Child':
                filters2 ="RowKey eq '" + element.ParentRowKey + "' and PartitionKey eq '" + element.ParentPartitionKey + "'";
              break;
            case 'Parent':
                filters2 ="RowKey eq '" + element.ChildRowKey + "' and PartitionKey eq '" + element.ChildPartitionKey + "'";
              break;
          }

          caller.SearchService.ResultsByFilters(filters2).subscribe( { 
            next(sr) {             
              sr.value.forEach(element => {
                s.row1 = element.Name;
                if (element.Surname != null)
                {
                  s.row1 += " " + element.Surname;
                }

                if (element.Mail != null)
                  s.row3  = element.Mail;

                if (element.ResourceType != null)
                  s.row3  = element.ResourceType;
                  
                s.key = element.Key;

                caller.IconService.getSvg(element.Type).subscribe( data =>
                  {
                    data = data.replace("<svg ", "<svg style='width: 32px; height: 32px;' ")
                    var sanit = caller.sanitizer.bypassSecurityTrustHtml(data);
                    s.iconSVG = sanit;
                  }
                ); 
              });
              caller.IsWaitChild=false;
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