import { Component, OnInit } from '@angular/core';
import { AzureSearchService } from '../azure-search.service'

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
  constructor( private SearchService:AzureSearchService) { }


  ngOnInit(): void {
  }

  searchString: string;
  
  suggestions: Suggest[];

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
      
      /*
      var max = Math.floor(Math.random() * 10) + 1; // from 1 to 10
      for (var i = 0; i<max; i++)
      {
        var s = new Suggest();

        s.key = "12345";
        s.name = this.searchString + " lorem ipsur dixit";

        this.suggestions.push(s);
      }
      */
  }
}
