import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { AzureSearchService } from '../../azure-search.service'

export class Suggest {
  name: string;
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
    private router: Router) { }


  ngOnInit(): void {
  }

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
  
                s.key = element.Key
                s.name = element["@search.text"];
  
                
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
