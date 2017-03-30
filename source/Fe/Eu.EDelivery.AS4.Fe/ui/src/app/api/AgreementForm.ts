import { FormBuilder, FormGroup } from '@angular/forms';

import { Agreement } from './Agreement';

export class AgreementForm {
    public static getForm(formBuilder: FormBuilder, current: Agreement): FormGroup {
        return formBuilder.group({
            [Agreement.FIELD_value]: [current && current.value],
            [Agreement.FIELD_type]: [current && current.type],
            [Agreement.FIELD_pModeId]: [current && current.pModeId],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Agreement) {
        form.get(Agreement.FIELD_value).reset({ value: current && current.value, disabled: !!!current && form.parent.disabled });
        form.get(Agreement.FIELD_type).reset({ value: current && current.type, disabled: !!!current && form.parent.disabled });
        form.get(Agreement.FIELD_pModeId).reset({ value: current && current.pModeId, disabled: !!!current && form.parent.disabled });
    }
}