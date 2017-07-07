import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DuplicateElimination } from './DuplicateElimination';

export class DuplicateEliminationForm {
    public static getForm(formBuilder: FormBuilder, current: DuplicateElimination): FormGroup {
        return formBuilder.group({
            isEnabled: [!!(current && current.isEnabled)],
        });
    }
}
