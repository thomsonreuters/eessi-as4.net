import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Transformer } from './Transformer';

export class TransformerForm {
    public static getForm(formBuilder: FormBuilder, current: Transformer): FormGroup {
        return formBuilder.group({
            type: [current && current.type],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Transformer) {
        form.removeControl('type');
        form.addControl('type', formBuilder.control(current && current.type));

    }
}
