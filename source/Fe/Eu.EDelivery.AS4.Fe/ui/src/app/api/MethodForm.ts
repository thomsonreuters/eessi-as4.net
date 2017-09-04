import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ItemType } from './ItemType';
import { Method } from './Method';
import { ParameterForm } from './ParameterForm';
import { FormWrapper } from './../common/form.service';

export class MethodForm {
    public static getForm(formBuilder: FormWrapper, current: Method | null, path: string, runtime: ItemType[], isDisabled?: boolean): FormWrapper {
        let form = formBuilder
            .group({
                [Method.FIELD_type]: [formBuilder.createFieldValue(current, Method.FIELD_type, path, 0, runtime), Validators.required],
                [Method.FIELD_parameters]: formBuilder.formBuilder.array(!!!(current && current.parameters) ? [] : current.parameters.map(item => ParameterForm.getForm(formBuilder.formBuilder, item))),
            });

        return form;
    }
}
