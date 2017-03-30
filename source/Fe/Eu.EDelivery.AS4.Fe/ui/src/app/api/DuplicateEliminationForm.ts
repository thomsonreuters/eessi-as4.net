import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DuplicateElimination } from './DuplicateElimination';

export class DuplicateEliminationForm {
    public static getForm(formBuilder: FormBuilder, current: DuplicateElimination): FormGroup {
        return formBuilder.group({
            isEnabled: [!!(current && current.isEnabled)],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: DuplicateElimination) {
        form.get(DuplicateElimination.FIELD_isEnabled).reset({ value: current && current.isEnabled, disabled: !!!current && form.parent.disabled });
    }
}
