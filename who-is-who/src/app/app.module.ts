import { BrowserModule } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS, HttpClientModule } from "@angular/common/http";
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

import { MsalModule, MsalInterceptor, MsalService } from '@azure/msal-angular';

import { MainSearchBoxComponent } from './components/main-search-box/main-search-box.component';
import { ResultListComponent } from './components/result-list/result-list.component';
import { ResultComponent } from './components/result/result.component';
import { AzureElementsComponent} from './components/relateditems/azureelements/azureelements.component';

const isIE = window.navigator.userAgent.indexOf('MSIE ') > -1 || window.navigator.userAgent.indexOf('Trident/') > -1;

function getRedirectUri()
{
  return window.location.origin + "/";
}

@NgModule({
  declarations: [
    AppComponent,
    MainSearchBoxComponent,
    ResultListComponent,
    ResultComponent, 
    AzureElementsComponent
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
              path: ':key',
              component: ResultComponent
            } 
          ]
        }
      ]),
    MsalModule.forRoot({
      auth: {
        clientId: 'c45b71f2-8b57-43c0-8c8e-bcf7a00fa946', // This is your client ID
        authority: 'https://login.microsoftonline.com/common/', // This is your tenant ID
        redirectUri: getRedirectUri() // 'http://localhost:4200/'// This is your redirect URI
      },
      cache: {
        cacheLocation: 'localStorage',
        storeAuthStateInCookie: isIE, // Set to true for Internet Explorer 11
      },
    }, {
      popUp: !isIE,
      consentScopes: [
        'user.read',
        'openid',
        'profile',
      ],
      unprotectedResources: [],
      protectedResourceMap: [
        ['https://graph.microsoft.com/v1.0/me', ['user.read']]
      ],
      extraQueryParameters: {}
    }),
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
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
 }

