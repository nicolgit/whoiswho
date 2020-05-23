import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { AzureSearchService } from '../../services/azure-search.service'
import { DomSanitizer, SafeHtml} from '@angular/platform-browser';
import { AzureIconService } from '../../services/azure-icons.service';

export class Suggest {
  iconSVG: SafeHtml;
  highlightText: SafeHtml;
  name: string;
  type: string;
  key: string;
}

@Component({
  selector: 'app-main-search-box',
  templateUrl: './main-search-box.component.html',
  styleUrls: ['./main-search-box.component.css']
})
export class MainSearchBoxComponent implements OnInit {
  constructor( 
    private SearchService:AzureSearchService,
    private IconService:AzureIconService,
    private router: Router,
    private sanitizer:DomSanitizer) { }


  ngOnInit(): void {
    var caller = this;
    this.SearchService.IndexSize().subscribe({
      next(sr)
      {
       caller.indexSize = sr;
      }
    });
  }

  indexSize:string;
  searchString: string;
  
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
    }
  }

  goSelectedAutoComplete (id)
  {
    this.router.navigateByUrl('/result/' + encodeURI(id) );
  }
}
