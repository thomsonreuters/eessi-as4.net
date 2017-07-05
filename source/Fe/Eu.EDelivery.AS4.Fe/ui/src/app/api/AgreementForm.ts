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
}
