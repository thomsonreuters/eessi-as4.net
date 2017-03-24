import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SendingPmode } from './SendingPmode';
import { SendingProcessingModeForm } from './SendingProcessingModeForm';

export class SendingPmodeForm {
    public static getForm(formBuilder: FormBuilder, current: SendingPmode): FormGroup {
        return formBuilder.group({
            type: [current && current.type],
            name: [current && current.name],
            pmode: SendingProcessingModeForm.getForm(formBuilder, current && current.pmode),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SendingPmode) {
        form.get(SendingPmode.FIELD_type).reset({ value: current && current.type, disabled: !!!current });
        form.get(SendingPmode.FIELD_name).reset({ value: current && current.name, disabled: !!!current });
        SendingProcessingModeForm.patchForm(formBuilder, <FormGroup>form.get(SendingPmode.FIELD_pmode), current && current.pmode);
        form.markAsPristine();
    }
}
