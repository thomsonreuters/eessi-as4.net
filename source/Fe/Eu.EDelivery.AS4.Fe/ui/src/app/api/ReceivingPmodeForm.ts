import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ItemType } from './ItemType';
import { FormWrapper } from './../common/form.service';
import { ReceivingPmode } from './ReceivingPmode';
import { ReceivingProcessingModeForm } from './ReceivingProcessingModeForm';

export class ReceivingPmodeForm {
    public static getForm(formBuilder: FormWrapper, current: ReceivingPmode, runtime: ItemType[]): FormWrapper {
        return formBuilder
            .group({
                [ReceivingPmode.FIELD_type]: [current && current.type],
                [ReceivingPmode.FIELD_name]: [current && current.name],
                [ReceivingPmode.FIELD_pmode]: ReceivingProcessingModeForm.getForm(formBuilder.subForm(ReceivingPmode.FIELD_pmode), current && current.pmode, runtime).form,
            });
    }
}
