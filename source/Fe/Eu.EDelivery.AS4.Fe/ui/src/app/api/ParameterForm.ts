import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ItemType } from './ItemType';
import { Parameter } from './Parameter';

export class ParameterForm {
    public static getForm(formBuilder: FormBuilder, current: Parameter, isDisabled?: boolean): FormGroup {
        return formBuilder.group({
            [Parameter.FIELD_name]: [{ value: current && current.name, disabled: isDisabled }, Validators.required],
            [Parameter.FIELD_value]: [{ value: current && current.value, disabled: isDisabled }, Validators.required],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Parameter) {
        form.removeControl(Parameter.FIELD_name);
        form.addControl(Parameter.FIELD_name, formBuilder.control(current && current.name));
        form.removeControl(Parameter.FIELD_value);
        form.addControl(Parameter.FIELD_value, formBuilder.control(current && current.value));
    }
}
