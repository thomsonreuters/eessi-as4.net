import { FormBuilder, FormGroup } from '@angular/forms';

import { ItemType } from './ItemType';
import { FormWrapper } from './../common/form.service';
import { Agreement } from './Agreement';

export class AgreementForm {
    public static getForm(formBuilder: FormWrapper, current: Agreement, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder.group({
            [Agreement.FIELD_value]: [formBuilder.createFieldValue(current, Agreement.FIELD_value, path, null, runtime)],
            [Agreement.FIELD_type]: [formBuilder.createFieldValue(current, Agreement.FIELD_type, path, null, runtime)]
        });
    }
}
