import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Http } from '@angular/http';
import { AuthenticationService } from '../authentication.service';
import { AuthenticationStore } from '../authentication.store';

@Component({
    selector: 'as4-login',
    templateUrl: './login.component.html'
})
export class LoginComponent {
    public username: string;
    public password: string;
    constructor(private http: Http, private activatedRoute: ActivatedRoute, private authenticationService: AuthenticationService, private authenticationStore: AuthenticationStore) {
        this.authenticationStore.changes.subscribe((result) => {
            console.log(result);
        });
        activatedRoute
            .queryParams
            .subscribe((result) => {
                let callback = result['callback'];
                if (!!callback) {
                    this.http
                        .get('api/authentication/externallogin?provider=Facebook&callback=true')
                        .subscribe((authenticationResult) => {
                            console.log(`Callback result token ${authenticationResult.json().access_token}`);
                        });
                }
            });
    }
    public login() {
        this.authenticationService
            .login(this.username, this.password)
            .filter((result) => !result)
            .subscribe(() => {
                this.username = '';
                this.password = '';            
            });
    }
}
