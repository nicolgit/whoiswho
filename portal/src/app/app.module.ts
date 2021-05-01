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

import { MsalGuard, MsalInterceptor, MsalBroadcastService, MsalInterceptorConfiguration, MsalModule, MsalService, MSAL_GUARD_CONFIG, MSAL_INSTANCE, MSAL_INTERCEPTOR_CONFIG, MsalGuardConfiguration, MsalRedirectComponent } from '@azure/msal-angular';
import { BrowserModule } from '@angular/platform-browser';

import { MainSearchBoxComponent } from './components/main-search-box/main-search-box.component';
import { ResultListComponent } from './components/result-list/result-list.component';
import { ResultComponent } from './components/result/result.component';
import { GenericElementsComponent } from './components/relateditems/genericelements/genericelements.component';
import { AppConfig } from './app.config';
import { Configuration } from 'msal';
import { BrowserCacheLocation, InteractionType, IPublicClientApplication, PublicClientApplication } from '@azure/msal-browser';

const isIE = window.navigator.userAgent.indexOf('MSIE ') > -1 || window.navigator.userAgent.indexOf('Trident/') > -1;

function getRedirectUri() {
  return window.location.origin + "/";
}

export function initializeApp(appConfig: AppConfig) {
  return () => appConfig.load();
}


export function MSALInstanceFactory(config: AppConfig): IPublicClientApplication {
  return new PublicClientApplication({
    auth: {
      clientId: config.settings.authentication.clientID, // This is your client ID
      authority: config.settings.authentication.authority, // This is your tenant ID
      redirectUri: getRedirectUri(), // This is your redirect URI
      postLogoutRedirectUri: getRedirectUri(),
      navigateToLoginRequestUrl: true,
    },
    cache: {
      cacheLocation: BrowserCacheLocation.LocalStorage,
      storeAuthStateInCookie: isIE, // set to true for IE 11
    },
    system: {
      loggerOptions: {
        piiLoggingEnabled: false
      }
    }
  });
}

export function MSALInterceptorConfigFactory(config: AppConfig): MsalInterceptorConfiguration {
  const protectedResourceMap = new Map<string, Array<string>>();

  for (var resource of config.settings.protectedResources) {
    protectedResourceMap.set(resource.endpoint, resource.scopes);
  }

  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap
  };
}

export function MSALGuardConfigFactory(config: AppConfig): MsalGuardConfiguration {
  return {
    interactionType: InteractionType.Redirect,
    authRequest: {
      scopes: config.settings.MSALGuardScopes
    }
  };
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
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    },
    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory,
      deps: [AppConfig]
    },
    {
      provide: MSAL_GUARD_CONFIG,
      useFactory: MSALGuardConfigFactory,
      deps: [AppConfig]
    },
    {
      provide: MSAL_INTERCEPTOR_CONFIG,
      useFactory: MSALInterceptorConfigFactory,
      deps: [AppConfig]
    },
    MsalService,
    MsalGuard,
    MsalBroadcastService,
  ],
  bootstrap: [AppComponent, MsalRedirectComponent]
})
export class AppModule {
}

