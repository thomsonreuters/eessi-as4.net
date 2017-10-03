import { Router } from '@angular/router';
import { Injectable } from '@angular/core';

import { TOKENSTORE } from './token';
import { AuthenticationStore } from './authentication.store';

@Injectable()
export class LogoutService {
    constructor(private router: Router) { }
    public logout() {
        this.router.navigate(['/login']);
    }
}
