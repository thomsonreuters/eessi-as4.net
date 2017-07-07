import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { Transformer } from './Transformer';

export class TransformerForm {
    public static getForm(formBuilder: FormWrapper, current: Transformer | undefined): FormWrapper {
        return formBuilder
            .group({
                type: [current && current.type],
            });
    }
}
