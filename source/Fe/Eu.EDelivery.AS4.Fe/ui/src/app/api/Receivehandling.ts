import { ISettingsState } from './../settings/settings.store';
/* tslint:disable */
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Method } from "./Method";

export class Receivehandling {
    notifyMessageConsumer: boolean;
    notifyMethod: Method;

    static FIELD_notifyMessageConsumer: string = 'notifyMessageConsumer';
    static FIELD_notifyMethod: string = 'notifyMethod';  
}
