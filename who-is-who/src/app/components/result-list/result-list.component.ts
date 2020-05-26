import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeHtml} from '@angular/platform-browser';

import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { AzureSearchService } from '../../services/azure-search.service';
import { AzureIconService } from '../../services/azure-icons.service';

export class Result {
  iconSVG: SafeHtml;
  
  row1: string;
  row2: string;
  
  key: string;
}

export class Suggest {
  iconSVG: SafeHtml;
  highlightText: SafeHtml;
  name: string;
  type: string;
  key: string;
}


@Component({
  selector: 'app-result-list',
  templateUrl: './result-list.component.html',
  styleUrls: ['./result-list.component.css']
})
export class ResultListComponent implements OnInit {

  constructor(  
    private SearchService:AzureSearchService,
    private IconService:AzureIconService,
    private route: ActivatedRoute,
    private router: Router,
    private sanitizer:DomSanitizer) { }

  ngOnInit(): void {
      this.searchString = this.route.snapshot.paramMap.get('search_for');
      
      this.search();
  }

  searchString: string;
  searchResults: Result[];

  suggestions: Suggest[];
  selectedAutocomplete: Suggest;

  doSuggestions()
  {
    var caller = this;
    caller.suggestions = [];
    
    if (typeof this.searchString != 'undefined' && this.searchString.trim())
    {
      this.SearchService.Suggestions(this.searchString).subscribe( {
        next(sr) {
            sr.value.forEach(element => {
              var s = new Suggest();

              s.key = element.Key;
              s.name = element.Name;
              s.type = element.Type;
              s.highlightText = caller.sanitizer.bypassSecurityTrustHtml(element["@search.text"]);
              caller.IconService.getSvg(element.Type).subscribe( data =>
                {
                  data = data.replace("<svg ", "<svg style='width: 16; height: 16;' ")
                  var sanit = caller.sanitizer.bypassSecurityTrustHtml(data);
                  
                  s.iconSVG = sanit;
                }
              );

              caller.suggestions.push(s);
            });    
        },
        error(msg) {
          console.log('error: ', msg);
        }
      });
    }
  }

  goSearch() {
    if (typeof this.searchString != 'undefined' && this.searchString.trim())
    {
      this.router.navigateByUrl('/search/' + encodeURI(this.searchString) );
      this.search();
    }
  }

  goSelectedAutoComplete (id)
  {
    this.router.navigateByUrl('/result/' + encodeURI(id) );
  }

  private search()
  {
    var caller = this;
    caller.searchResults = [];
    caller.suggestions = [];
    
    var s = caller.searchString;
    if (!s.endsWith("*"))
    {
      s += "*";
    }

    this.SearchService.ResultsByText(s).subscribe( {
      next(sr) {
        sr.value.forEach(element => {
          var s = new Result();

          s.key = element.Key
          s.row1 = element.Name;
          if (element.Surname != null)
          {
            s.row1 += " " + element.Surname;
          }
                    
          s.row2 = element.Type;
          if (element.ResourceType != null)
          {
            s.row2 += " - " + element.ResourceType;
          }

          caller.IconService.getSvg(element.Type).subscribe( data =>
            {
              data = data.replace("<svg ", "<svg style='width: 32; height: 32;' ")
              var sanit = caller.sanitizer.bypassSecurityTrustHtml(data);
              
              s.iconSVG = sanit;
            }
          );

          caller.searchResults.push(s);
        });    
      },
      error(msg) {
        console.log('error: ', msg);
      }
    });
  }
}
