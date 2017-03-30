import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReceptionAwareness } from './ReceptionAwareness';

export class ReceptionAwarenessForm {
    public static getForm(formBuilder: FormBuilder, current: ReceptionAwareness): FormGroup {
        let form = formBuilder.group({
            [ReceptionAwareness.FIELD_isEnabled]: [!!(current && current.isEnabled), Validators.required],
            [ReceptionAwareness.FIELD_retryCount]: [(current == null || current.retryCount == null) ? 0 : current.retryCount, Validators.required],
            [ReceptionAwareness.FIELD_retryInterval]: [(current == null || current.retryInterval == null) ? 0 : current.retryInterval, Validators.required],
        });
        ReceptionAwarenessForm.setupForm(form);
        return form;
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceptionAwareness) {
        form.get(ReceptionAwareness.FIELD_isEnabled).reset({ value: current && current.isEnabled, disabled: !!!current });
        form.get(ReceptionAwareness.FIELD_retryCount).reset({ value: current && current.retryCount, disabled: !!!current || !current.isEnabled });
        form.get(ReceptionAwareness.FIELD_retryInterval).reset({ value: current && current.retryInterval, disabled: !!!current || !current.isEnabled });
    }

    private static setupForm(form: FormGroup) {
        form.get(ReceptionAwareness.FIELD_isEnabled).valueChanges.subscribe((result) => this.processEnabled(form));
    }

    private static processEnabled(form: FormGroup) {
        let isEnabled = form.get(ReceptionAwareness.FIELD_isEnabled).value;
        if (isEnabled) {
            form.get(ReceptionAwareness.FIELD_retryCount).enable();
            form.get(ReceptionAwareness.FIELD_retryInterval).enable();
        } else {
            form.get(ReceptionAwareness.FIELD_retryCount).disable();
            form.get(ReceptionAwareness.FIELD_retryInterval).disable();
        }
    }
}
