import { Component, OnInit } from '@angular/core';

import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { AzureSearchService } from '../../services/azure-search.service'

export class Result {
  row1: string;
  row2: string;
  iconSVG: string;
  key: string;
}

export class Suggest {
  name: string;
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
    private route: ActivatedRoute,
    private router: Router) { }

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

    this.SearchService.Results(s).subscribe( {
      next(sr) {
        sr.value.forEach(element => {
          var s = new Result();

          s.key = element.Key
          s.row1 = element.Name;
          s.row2 = element.Type;
          s.iconSVG = "<b>ciao</b><i>ciao</i>";

          caller.searchResults.push(s);
        });    
      },
      error(msg) {
        console.log('error: ', msg);
      }
    });
  }
}
