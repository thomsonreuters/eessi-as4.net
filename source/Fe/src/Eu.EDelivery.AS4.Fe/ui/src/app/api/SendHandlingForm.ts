import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SendHandling } from './SendHandling';
import { MethodForm } from './MethodForm';

export class SendHandlingForm {
    public static getForm(formBuilder: FormBuilder, current: SendHandling): FormGroup {
        let form = formBuilder.group({
            [SendHandling.FIELD_notifyMessageProducer]: [!!(current && current.notifyMessageProducer)],
            [SendHandling.FIELD_notifyMethod]: MethodForm.getForm(formBuilder, current && current.notifyMethod),
        });
        this.setupForm(form);
        return form;
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SendHandling) {
        form.get(SendHandling.FIELD_notifyMessageProducer).reset({ value: current && current.notifyMessageProducer, disabled: !!!current });
        MethodForm.patchForm(formBuilder, <FormGroup>form.get(SendHandling.FIELD_notifyMethod), current && current.notifyMethod, !!!current || !current.notifyMessageProducer);
    }

    private static setupForm(form: FormGroup) {
        let notifyProducer = form.get(SendHandling.FIELD_notifyMessageProducer);
        let notifyMethod = form.get(SendHandling.FIELD_notifyMethod);
        let enable = () => notifyMethod.enable();
        let disable = () => notifyMethod.disable();
        let toggle = (result: boolean) => {
            if (result) {
                enable();
            } else {
                disable();
            }
        };

        toggle(notifyProducer.value);
        notifyProducer.valueChanges.subscribe((result) => toggle(result));
    }
}
