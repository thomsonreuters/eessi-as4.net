import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessageProperty } from './MessageProperty';

export class MessagePropertyForm {
    public static getForm(formBuilder: FormBuilder, current: MessageProperty): FormGroup {
        return formBuilder.group({
            name: [current && current.name],
            type: [current && current.type],
            value: [current && current.value],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: MessageProperty) {
        form.removeControl(MessageProperty.FIELD_name);
        form.addControl(MessageProperty.FIELD_name, formBuilder.control(current && current.name));
        form.removeControl(MessageProperty.FIELD_type);
        form.addControl(MessageProperty.FIELD_value, formBuilder.control(current && current.type));
        form.removeControl(MessageProperty.FIELD_value);
        form.addControl(MessageProperty.FIELD_value, formBuilder.control(current && current.value));
    }
}
