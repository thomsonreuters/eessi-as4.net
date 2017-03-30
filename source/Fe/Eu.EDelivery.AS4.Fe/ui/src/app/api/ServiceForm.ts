import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Service } from './Service';

export class ServiceForm {
    public static getForm(formBuilder: FormBuilder, current: Service): FormGroup {
        return formBuilder.group({
            value: [current && current.value],
            type: [current && current.type],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Service) {
        form.get(Service.FIELD_value).reset({ value: current && current.value, disabled: !!!current && form.parent.disabled });
        form.get(Service.FIELD_type).reset({ value: current && current.type, disabled: !!!current && form.parent.disabled });
    }
}
