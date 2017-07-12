import { FormBuilder, FormGroup } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { Agreement } from './Agreement';

export class AgreementForm {
    public static getForm(formBuilder: FormWrapper, current: Agreement): FormWrapper {
        return formBuilder.group({
            [Agreement.FIELD_value]: [current && current.value],
            [Agreement.FIELD_type]: [current && current.type],
            [Agreement.FIELD_pModeId]: [current && current.pModeId],
        });
    }
}
