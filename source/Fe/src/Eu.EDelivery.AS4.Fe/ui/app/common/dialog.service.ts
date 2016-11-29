import { Injectable } from '@angular/core';

@Injectable()
export class DialogService {
    public prompt(message: string): string {
        let result = prompt(message, '');
        return result;
    }
}
