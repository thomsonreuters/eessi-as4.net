import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ItemType } from './ItemType';
import { FormWrapper } from './../common/form.service';
import { Service } from './Service';

export class ServiceForm {
    public static getForm(formBuilder: FormWrapper, current: Service, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder.group({
            value: [formBuilder.createFieldValue(current, Service.FIELD_value, path, null, runtime)],
            type: [formBuilder.createFieldValue(current, Service.FIELD_type, path, null, runtime)],
        });
    }
}
