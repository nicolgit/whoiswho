import { Component, Inject, OnInit } from '@angular/core';
import { Location } from "@angular/common";
import { MsalService, MsalBroadcastService, MSAL_GUARD_CONFIG, MsalGuardConfiguration, MsalCustomNavigationClient } from '@azure/msal-angular';
import { HttpClient } from '@angular/common/http';
import { Logger, CryptoUtils } from 'msal';

import { AppStatics } from './model/appstatics';
import { Router } from '@angular/router';
import { AuthenticationResult, InteractionStatus, PopupRequest, RedirectRequest } from '@azure/msal-browser';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  constructor(
    @Inject(MSAL_GUARD_CONFIG) private msalGuardConfig: MsalGuardConfiguration,
    private msalBroadcastService: MsalBroadcastService,   
    private authService: MsalService,
    private http: HttpClient,
    private router: Router,
    private location: Location) { 
       // Custom navigation set for client-side navigation. See performance doc for details: https://github.com/AzureAD/microsoft-authentication-library-for-js/tree/dev/lib/msal-angular/docs/v2-docs/performance.md
        const customNavigationClient = new MsalCustomNavigationClient(authService, this.router, this.location);
        this.authService.instance.setNavigationClient(customNavigationClient);
    }
  
  title = "Azure Enterprise App's Search Engine";
  isIframe = false;
  loginDisplay = false;
  profile;

  private readonly _destroying$ = new Subject<void>();

  ngOnInit() {
    this.isIframe = window !== window.parent && !window.opener;

    this.msalBroadcastService.inProgress$
      .pipe(
        filter((status: InteractionStatus) => status === InteractionStatus.None),
        takeUntil(this._destroying$)
      )
      .subscribe(() => {
        this.setLoginDisplay();
      })

    this.isIframe = window !== window.parent && !window.opener;

    //this.getProfile();
  }

  setLoginDisplay() {
    this.loginDisplay = this.authService.instance.getAllAccounts().length > 0;
  }

  loginRedirect() {
    if (this.msalGuardConfig.authRequest){
      this.authService.loginRedirect({...this.msalGuardConfig.authRequest} as RedirectRequest);
    } else {
      this.authService.loginRedirect();
    }
  }

  loginPopup() {
    if (this.msalGuardConfig.authRequest){
      this.authService.loginPopup({...this.msalGuardConfig.authRequest} as PopupRequest)
        .subscribe((response: AuthenticationResult) => {
          this.authService.instance.setActiveAccount(response.account);
        });
      } else {
        this.authService.loginPopup()
          .subscribe((response: AuthenticationResult) => {
            this.authService.instance.setActiveAccount(response.account);
      });
    }
  }

  logout(popup?: boolean) {
    if (popup) {
      this.authService.logoutPopup({
        mainWindowRedirectUri: "/"
      });
    } else {
      this.authService.logoutRedirect();
    }
  }

  ngOnDestroy(): void {
    this._destroying$.next(null);
    this._destroying$.complete();
  }

  /*
  getProfile() {
    this.http.get(AppStatics.MSGRAPH_ME)
      .toPromise().then(profile => {
        this.profile = profile;
        this.checkoutAccount();
      });
  }*/
}
