import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SendingPmode } from './SendingPmode';
import { SendingProcessingModeForm } from './SendingProcessingModeForm';
import { FormWrapper } from './../common/form.service';

export class SendingPmodeForm {
    public static getForm(formWrapper: FormWrapper, current: SendingPmode): FormWrapper {
        return formWrapper
            .group({
                type: [current && current.type],
                name: [current && current.name],
                pmode: SendingProcessingModeForm.getForm(formWrapper.subForm('pmode'), current && current.pmode).form
            });
    }
    /// Patch up all the formArray controls
    public static patchForm(formWrapper: FormWrapper, form: FormGroup, current: SendingPmode) {
        form.get(SendingPmode.FIELD_type).reset({ value: current && current.type, disabled: !!!current });
        form.get(SendingPmode.FIELD_name).reset({ value: current && current.name, disabled: !!!current });
        SendingProcessingModeForm.patchForm(formWrapper.formBuilder, <FormGroup>form.get(SendingPmode.FIELD_pmode), current && current.pmode);
        form.markAsPristine();
    }
}
