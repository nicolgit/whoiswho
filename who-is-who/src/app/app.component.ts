import { Component, OnInit } from '@angular/core';
import { MsalService, BroadcastService } from '@azure/msal-angular';
import { HttpClient } from '@angular/common/http';

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

  private graphMeEndpoint = "https://graph.microsoft.com/v1.0/me";

  title = 'who-is-who';
  isIframe = false;
  loggedIn = false;
  userName = "";

  ngOnInit() { 
    var caller = this;

    this.isIframe = window !== window.parent && !window.opener;
    this.checkAccount();

    if (this.loggedIn == false)
    {
      this.login();
    }

    this.authService.acquireTokenSilent({
      scopes: this.authService.getScopesForEndpoint(this.graphMeEndpoint)
    })

    this.http.get(this.graphMeEndpoint).subscribe({
      next(sr)
      {
        caller.userName = sr['displayName'];
      }
    });

    this.broadcastService.subscribe('msal:loginSuccess', () => {
      this.checkAccount();
    });

    this.authService.handleRedirectCallback((authError, response) => {
      if (authError) {
        console.error('Redirect Error: ', authError.errorMessage);
        return;
      }

      console.log('Redirect Success: ', response.accessToken);
    });
  }

  login() {
    this.authService.loginRedirect({
      extraScopesToConsent: ["user.read", "openid", "profile"]
    });
  }

  logout() {
    this.authService.logout();
  }

  checkAccount() {
    this.loggedIn = !!this.authService.getAccount();
  }
}
