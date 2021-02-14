import { Component, OnInit } from '@angular/core';
import { MsalService, BroadcastService } from '@azure/msal-angular';
import { HttpClient } from '@angular/common/http';
import { Logger, CryptoUtils } from 'msal';

import { AppStatics } from './model/appstatics';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  constructor(
    private broadcastService: BroadcastService, 
    private authService: MsalService,
    private http: HttpClient) { 

    }
  
  title = "Azure Enterprise App's Search Engine";
  isIframe = false;
  loggedIn = false;
  profile;

  ngOnInit() {
    this.isIframe = window !== window.parent && !window.opener;

    this.checkoutAccount();

    this.broadcastService.subscribe('msal:loginSuccess', () => {
      this.checkoutAccount();
      this.getProfile();
    });

    this.authService.handleRedirectCallback((authError, response) => {
      if (authError) {
        console.error('Redirect Error: ', authError.errorMessage);
        return;
      }

      console.log('Redirect Success: ', response);
    });

    this.authService.setLogger(new Logger((logLevel, message, piiEnabled) => {
      console.log('MSAL Logging: ', message);
    }, {
      correlationId: CryptoUtils.createNewGuid(),
      piiLoggingEnabled: false
    }));

    this.getProfile();
  }

  checkoutAccount() {
    this.loggedIn = !!this.authService.getAccount() && this.profile!=null;
  }

  login() { 
    const isIE = window.navigator.userAgent.indexOf('MSIE ') > -1 || window.navigator.userAgent.indexOf('Trident/') > -1;

    if (isIE) {
      this.authService.loginRedirect();
    } else {
      this.authService.loginPopup();
    }
  }

  logout() {
    this.authService.logout();
  }

  getProfile() {
    this.http.get(AppStatics.MSGRAPH_ME)
      .toPromise().then(profile => {
        this.profile = profile;
        this.checkoutAccount();
      });
  }
}
