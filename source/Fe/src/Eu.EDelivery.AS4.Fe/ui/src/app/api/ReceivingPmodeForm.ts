import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReceivingPmode } from './ReceivingPmode';
import { ReceivingProcessingModeForm } from './ReceivingProcessingModeForm';

export class ReceivingPmodeForm {
    public static getForm(formBuilder: FormBuilder, current: ReceivingPmode): FormGroup {
        return formBuilder.group({
            [ReceivingPmode.FIELD_type]: [current && current.type],
            [ReceivingPmode.FIELD_name]: [current && current.name],
            [ReceivingPmode.FIELD_pmode]: ReceivingProcessingModeForm.getForm(formBuilder, current && current.pmode),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceivingPmode) {
        form.get(ReceivingPmode.FIELD_type).reset({ value: current && current.type, disabled: !!!current });
        form.get(ReceivingPmode.FIELD_name).reset({ value: current && current.name, disabled: !!!current });
        ReceivingProcessingModeForm.patchForm(formBuilder, <FormGroup>form.get(ReceivingPmode.FIELD_pmode), current && current.pmode);
        form.updateValueAndValidity();
    }
}
