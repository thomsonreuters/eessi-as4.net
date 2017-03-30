import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Deliver } from './Deliver';
import { MethodForm } from './MethodForm';

export class DeliverForm {
    public static getForm(formBuilder: FormBuilder, current: Deliver): FormGroup {
        let form = formBuilder.group({
            isEnabled: [{ value: !!(current && current.isEnabled), disabled: !!current }],
            payloadReferenceMethod: MethodForm.getForm(formBuilder, current && current.payloadReferenceMethod),
            deliverMethod: MethodForm.getForm(formBuilder, current && current.deliverMethod),
        });
        DeliverForm.setupForm(form);
        return form;
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Deliver) {
        form.get(Deliver.FIELD_isEnabled).reset({ value: current && current.isEnabled, disabled: !!!current && form.parent.disabled });
        MethodForm.patchForm(formBuilder, <FormGroup>form.get(Deliver.FIELD_deliverMethod), current && current.deliverMethod, current && !current.isEnabled);
        MethodForm.patchForm(formBuilder, <FormGroup>form.get(Deliver.FIELD_payloadReferenceMethod), current && current.payloadReferenceMethod, current && !current.isEnabled);
    }

    public static setupForm(formGroup: FormGroup) {
        let payload = formGroup.get(Deliver.FIELD_payloadReferenceMethod);
        let method = formGroup.get(Deliver.FIELD_deliverMethod);
        let enable = () => {
            payload.enable({ onlySelf: true });
            method.enable({ onlySelf: true });
        };
        let disable = () => {
            payload.disable({ onlySelf: false });
            method.disable({ onlySelf: false });
        };

        let isEnabled = formGroup.get(Deliver.FIELD_isEnabled);
        if (isEnabled.value) {
            payload.enable();
        } else {
            payload.disable();
        }
        isEnabled
            .valueChanges
            .filter(() => !(formGroup && formGroup.parent && formGroup.parent.disabled))
            .subscribe((result) => {
                if (!result) {
                    disable();
                } else {
                    enable();
                }
                formGroup.markAsDirty();
            });
    }
}