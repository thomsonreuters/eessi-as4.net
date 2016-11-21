import { Component, ViewEncapsulation } from '@angular/core';

import { AuthenticationService } from './authentication/authentication.service';

@Component({
  selector: 'as4-header',
  encapsulation: ViewEncapsulation.None,
  templateUrl: './header.component.html'
})
export class HeaderComponent {
  constructor(private authenticationService: AuthenticationService) {

  }
  logout() {
    this.authenticationService.logout();
  }
}