import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Method } from './Method';
import { ParameterForm } from './ParameterForm';
import { FormWrapper } from './../common/form.service';

export class MethodForm {
    public static getForm(formBuilder: FormWrapper, current: Method, isDisabled?: boolean): FormWrapper {
        let form = formBuilder
            .group({
                [Method.FIELD_type]: [current && current.type, Validators.required],
                [Method.FIELD_parameters]: formBuilder.formBuilder.array(!!!(current && current.parameters) ? [] : current.parameters.map(item => ParameterForm.getForm(formBuilder.formBuilder, item))),
            });

        return form;
    }
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Method, disabled?: boolean) {
        form.get(Method.FIELD_type).reset({ value: current && current.type, disabled: disabled });
        form.removeControl(Method.FIELD_parameters);
        form.addControl(Method.FIELD_parameters, formBuilder.array(!!!(current && current.parameters) ? [] : current.parameters.map(item => ParameterForm.getForm(formBuilder, item, disabled))));
    }
}
