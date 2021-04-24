import { BrowserModule } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS, HttpClientModule } from "@angular/common/http";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from './material.module';

import { FlexLayoutModule } from '@angular/flex-layout/';

import { NgModule, APP_INITIALIZER } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { MsalModule, MsalInterceptor, MsalService, MsalGuard, MSAL_CONFIG, MSAL_CONFIG_ANGULAR, MsalAngularConfiguration } from '@azure/msal-angular';

import { MainSearchBoxComponent } from './components/main-search-box/main-search-box.component';
import { ResultListComponent } from './components/result-list/result-list.component';
import { ResultComponent } from './components/result/result.component';
import { GenericElementsComponent } from './components/relateditems/genericelements/genericelements.component';
import { AppConfig } from './app.config';
import { Configuration } from 'msal';

const isIE = window.navigator.userAgent.indexOf('MSIE ') > -1 || window.navigator.userAgent.indexOf('Trident/') > -1;

function getRedirectUri() {
  return window.location.origin + "/";
}

export function initializeApp(appConfig: AppConfig) {
  return () => appConfig.load();
}

export function msalConfigFactory(config: AppConfig) {
  const auth = {
    auth: {
      clientId: config.settings.authentication.clientID, // This is your client ID
      authority: config.settings.authentication.authority, // This is your tenant ID
      validateAuthority: true,
      redirectUri: getRedirectUri(), // This is your redirect URI
      postLogoutRedirectUri: getRedirectUri(),
      navigateToLoginRequestUrl: true,
    },
    cache: {
      cacheLocation: 'localStorage',
      storeAuthStateInCookie: isIE, // Set to true for Internet Explorer 11
    },
  };
  return (auth as Configuration);
}

export function msalAngularConfigFactory(config: AppConfig): MsalAngularConfiguration {
  const auth = {
    popUp: !isIE,
    consentScopes: [
      'user.read',
      'openid',
      'profile',
      'api://84daf407-55bf-4241-8512-960393de9fe4/access_as_user'
    ],
    unprotectedResources: [],
    protectedResourceMap: [
      ['https://graph.microsoft.com/v1.0/me', ['user.read']],
      ['http://localhost:7071', ['api://84daf407-55bf-4241-8512-960393de9fe4/access_as_user']],
      ['assets/', null]
    ],
    extraQueryParameters: {}
  };
  return (auth as MsalAngularConfiguration);
}


@NgModule({
  declarations: [
    AppComponent,
    MainSearchBoxComponent,
    ResultListComponent,
    ResultComponent,
    GenericElementsComponent
  ],
  imports: [
    BrowserModule,
    RouterModule.forRoot([
      { path: '', component: MainSearchBoxComponent, canActivate: [MsalGuard] },
      {
        path: 'search',
        children: [
          { path: '', component: ResultListComponent, canActivate: [MsalGuard] },
          { path: ':search_for', component: ResultListComponent, canActivate: [MsalGuard] }
        ]
      },
      {
        path: 'result',
        children: [
          { path: '', component: ResultComponent },
          { path: ':key', component: ResultComponent }
        ]
      },
    ]),
    MsalModule,
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
    AppConfig,
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [AppConfig],
      multi: true
    }, {
      provide: MSAL_CONFIG,
      useFactory: msalConfigFactory,
      deps: [AppConfig]
    },
    {
      provide: MSAL_CONFIG_ANGULAR,
      useFactory: msalAngularConfigFactory,
      deps: [AppConfig]
    },
    MsalService,
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

