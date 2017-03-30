import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Receivehandling } from './Receivehandling';
import { MethodForm } from './MethodForm';

export class ReceivehandlingForm {
    public static getForm(formBuilder: FormBuilder, current: Receivehandling): FormGroup {
        let form = formBuilder.group({
            notifyMessageConsumer: [!!(current && current.notifyMessageConsumer)],
            notifyMethod: MethodForm.getForm(formBuilder, current && current.notifyMethod),
        });

        this.setupForm(form);
        return form;
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Receivehandling) {
        form.get(Receivehandling.FIELD_notifyMessageConsumer).reset({ value: current && current.notifyMessageConsumer, disabled: !!!current });
        MethodForm.patchForm(formBuilder, <FormGroup>form.get(Receivehandling.FIELD_notifyMethod), current && current.notifyMethod, !!!current || !current.notifyMessageConsumer);
    }

    private static setupForm(form: FormGroup) {
        if (form.get(Receivehandling.FIELD_notifyMessageConsumer).value) {
            form.get(Receivehandling.FIELD_notifyMethod).enable();
        } else {
            form.get(Receivehandling.FIELD_notifyMethod).disable();
        }

        form.get(Receivehandling.FIELD_notifyMessageConsumer).valueChanges.subscribe(result => {
            if (result) form.get(Receivehandling.FIELD_notifyMethod).enable();
            else form.get(Receivehandling.FIELD_notifyMethod).disable();
        });
    }
}