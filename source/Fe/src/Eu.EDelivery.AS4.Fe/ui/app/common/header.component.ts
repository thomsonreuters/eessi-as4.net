import { Component, ViewEncapsulation } from '@angular/core';

import { AuthenticationService, AuthenticationStore } from '../authentication/authentication.service';

@Component({
  selector: 'as4-header',
  encapsulation: ViewEncapsulation.None,
  templateUrl: './header.component.html'
})
export class HeaderComponent {
  public isLoggedIn: boolean;
  constructor(private authenticationService: AuthenticationService, private authenticationStore: AuthenticationStore) {
    authenticationStore.changes.subscribe(result => this.isLoggedIn = result.loggedin);
  }
  logout() {
    this.authenticationService.logout();
  }
}
