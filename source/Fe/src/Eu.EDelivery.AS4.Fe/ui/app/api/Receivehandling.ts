import { ISettingsState } from './../settings/settings.store';
/* tslint:disable */
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Method } from "./Method";

export class Receivehandling {
    notifyMessageConsumer: boolean;
    notifyMethod: Method;

    static FIELD_notifyMessageConsumer: string = 'notifyMessageConsumer';
    static FIELD_notifyMethod: string = 'notifyMethod';

    static getForm(formBuilder: FormBuilder, current: Receivehandling): FormGroup {
        let form = formBuilder.group({
            notifyMessageConsumer: [!!(current && current.notifyMessageConsumer)],
            notifyMethod: Method.getForm(formBuilder, current && current.notifyMethod),
        });

        setTimeout(() => this.setState(form));
        return form;
    }
    /// Patch up all the formArray controls
    static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Receivehandling) {
        form.removeControl('notifyMessageConsumer');
        form.addControl('notifyMessageConsumer', formBuilder.control(current && current.notifyMessageConsumer));
        form.removeControl('notifyMethod');
        form.addControl('notifyMethod', Method.getForm(formBuilder, current && current.notifyMethod));
        setTimeout(() => this.setState(form));
    }

    static setState(form: FormGroup) {
        if (form.get(this.FIELD_notifyMessageConsumer).value) {
            form.get(this.FIELD_notifyMethod).enable();
        }
        else {
            form.get(this.FIELD_notifyMethod).disable();
        }

        form.get(this.FIELD_notifyMessageConsumer).valueChanges.subscribe(result => {
            if (result) form.get(this.FIELD_notifyMethod).enable();
            else form.get(this.FIELD_notifyMethod).disable();
        });
    }
}
