import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Service } from './Service';

export class ServiceForm {
    public static getForm(formBuilder: FormBuilder, current: Service): FormGroup {
        return formBuilder.group({
            value: [current && current.value],
            type: [current && current.type],
        });
    }
}
