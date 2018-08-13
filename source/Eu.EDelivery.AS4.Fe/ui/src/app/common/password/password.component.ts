import { Component, Input } from '@angular/core';

@Component({
    selector: 'as4-password',
    templateUrl: 'password.component.html'
})

export class PasswordComponent {
    @Input() public invalid: boolean = false;
}
