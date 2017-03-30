import { Component, ViewEncapsulation, ChangeDetectionStrategy } from '@angular/core';

import { AuthenticationService } from '../../authentication/authentication.service';
import { AuthenticationStore } from '../../authentication/authentication.store';

@Component({
    selector: 'as4-header',
    encapsulation: ViewEncapsulation.None,
    templateUrl: './header.component.html',
    styleUrls: ['./header.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderComponent {
    public isLoggedIn: boolean;
    constructor(private authenticationService: AuthenticationService, private authenticationStore: AuthenticationStore) {
        authenticationStore.changes.subscribe((result) => this.isLoggedIn = result.loggedin);
    }
    public logout() {
        this.authenticationService.logout();
    }
}
