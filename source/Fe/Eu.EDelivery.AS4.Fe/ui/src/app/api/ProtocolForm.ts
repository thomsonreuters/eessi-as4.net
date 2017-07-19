import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Protocol } from './Protocol';
import { FormWrapper } from './../common/form.service';

export class ProtocolForm {
    public static getForm(formBuilder: FormWrapper, current: Protocol): FormWrapper {
        return formBuilder
            .group({
                [Protocol.FIELD_url]: [current && current.url, Validators.required]
            });
    }
}
