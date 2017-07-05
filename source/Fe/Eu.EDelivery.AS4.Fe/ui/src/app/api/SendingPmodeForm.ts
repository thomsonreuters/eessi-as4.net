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
}
