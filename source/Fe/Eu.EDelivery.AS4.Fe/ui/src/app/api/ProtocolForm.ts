import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Protocol } from './Protocol';
import { FormWrapper } from './../common/form.service';
import { ItemType } from './ItemType';

export class ProtocolForm {
    public static getForm(formBuilder: FormWrapper, current: Protocol, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder
            .group({
                [Protocol.FIELD_url]: [formBuilder.createFieldValue(current, Protocol.FIELD_url, path, null, runtime)]
            });
    }
}
