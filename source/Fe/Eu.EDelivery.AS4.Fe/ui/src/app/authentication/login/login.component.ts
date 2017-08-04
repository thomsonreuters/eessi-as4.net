import { Subscription } from 'rxjs/Subscription';
import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Http } from '@angular/http';
import { AuthenticationService } from '../authentication.service';
import { AuthenticationStore } from '../authentication.store';

@Component({
    selector: 'as4-login',
    templateUrl: './login.component.html',
    styles: [`
        .login-box {
            position: absolute;
            top: 50%;
            left: 50%;
            margin-top: -180px;
            margin-left: -180px;   
        }
    `]
})
export class LoginComponent implements OnDestroy {
    public username: string;
    public password: string;
    private _subscriptions: Subscription[] = new Array<Subscription>();
    constructor(private http: Http, private activatedRoute: ActivatedRoute, private authenticationService: AuthenticationService, private authenticationStore: AuthenticationStore) {
        this._subscriptions.push(this.authenticationStore.changes.subscribe((result) => {
            console.log(result);
        }));
        this._subscriptions.push(activatedRoute
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
            }));
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
    public ngOnDestroy() {
        this._subscriptions.forEach((sub) => sub.unsubscribe());
    }
}
