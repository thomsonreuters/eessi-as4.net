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
        form.get(this.FIELD_notifyMessageConsumer).reset({ value: current && current.notifyMessageConsumer, disabled: !!!current });
        Method.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_notifyMethod), current && current.notifyMethod, !!!current || !current.notifyMessageConsumer);
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
