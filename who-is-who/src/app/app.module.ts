import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from './material.module';

import { FlexLayoutModule } from '@angular/flex-layout/';

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { MainSearchBoxComponent } from './components/main-search-box/main-search-box.component';
import { ResultListComponent } from './components/result-list/result-list.component';
import { ResultComponent } from './components/result/result.component';

@NgModule({
  declarations: [
    AppComponent,
    MainSearchBoxComponent,
    ResultListComponent,
    ResultComponent,
  ],
  imports: [
    BrowserModule,
  RouterModule.forRoot([
      { path: '', component:MainSearchBoxComponent },
      { path: 'search', 
        children: [
          {
            path: '',
            component: ResultListComponent
          },
          {
            path: ':search_for',
            component: ResultListComponent
          } 
        ]
      },
      { path: 'result',
        children: [
          {
            path: '',
            component: ResultComponent
          },
          {
            path: ':id',
            component: ResultComponent
          } 
        ]
      }
     ]),
    HttpClientModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MaterialModule,
    MatFormFieldModule,
    MatInputModule,
    FlexLayoutModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
