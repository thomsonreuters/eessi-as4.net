import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { Service } from './Service';

export class ServiceForm {
    public static getForm(formBuilder: FormWrapper, current: Service): FormWrapper {
        return formBuilder.group({
            value: [current && current.value],
            type: [current && current.type],
        });
    }
}
