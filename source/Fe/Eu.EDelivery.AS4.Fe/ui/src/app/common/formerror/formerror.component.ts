import { Component, Input } from '@angular/core';
import { FormControl } from '@angular/forms';

@Component({
    selector: 'as4-form-error',
    templateUrl: './formerror.component.html',
    styleUrls: ['./formerror.component.scss']
})
export class FormErrorComponent {
    @Input() public control: FormControl;
    public get errors(): string[] {
        if (!this.control || !this.control.errors) {
            return [];
        }
        return Object.keys(this.control.errors!);
    }
}
