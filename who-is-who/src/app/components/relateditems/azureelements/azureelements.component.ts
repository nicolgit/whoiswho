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
  @Input() FilterCorelationType: string;
  @Input() FilterElementType:string;
  @Input() FilterElementTypeId:string;
  
  private _parentValue;
  @Input() 
  set ParentValue (pv:string) {    
    this._parentValue = pv;
    this.RefreshUI();
  }
  get ParentValue():string {
    return this._parentValue;
  }

  private _parentType;
  @Input()
  set ParentType (pt:string) {
    this._parentType = pt;
    this.RefreshUI();
  }
  get ParentType():string {
    return this._parentType;
  }

  private _parentKey;
  @Input()
  set ParentKey (pt:string) {
    this._parentKey = pt;
    this.RefreshUI();
  }
  get ParentKey():string {
    return this._parentKey;
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

    if (this._parentKey == null || this._parentType == null || this._parentValue == null)
      {
        // parameters still not ready
        return;
      }
    
      if (this._parentType == this.FilterElementType)
      {
        // not applicable
        return;
      }
    
    var filters = caller.FilterCorelationType + " and " + caller.ParentKey + " eq '" + caller.ParentValue + "'";

    this.SearchService.ResultsByFilters(filters).subscribe( {
      next(sr) {
        sr.value.forEach(element => {
          var s = new Result();

          s.key = element.Key
          s.row1 = "?";
          s.row2 = element.Name;
          s.row3 = "";

          var id = "";
         
          var filters2 = "Type eq '" + caller.FilterElementType + "' and RowKey eq '" + id + "'";

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