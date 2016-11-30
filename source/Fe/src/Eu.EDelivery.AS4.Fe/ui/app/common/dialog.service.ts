import { Injectable } from '@angular/core';

@Injectable()
export class DialogService {
    public prompt(message: string): string {
        let result = prompt(message, '');
        return result;
    }
    public confirm(message: string): boolean {
        return confirm(message);
    }
    public message(message: string) {
        alert(message);
    }
    public incorrectForm() {
        this.message('Input is invalid, please correct the red fields');
    }
}
