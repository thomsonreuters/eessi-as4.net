import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReceiveReliability } from './ReceiveReliability';
import { DuplicateEliminationForm } from './DuplicateEliminationForm';

export class ReceiveReliabilityForm {
    public static getForm(formBuilder: FormBuilder, current: ReceiveReliability): FormGroup {
        return formBuilder.group({
            duplicateElimination: DuplicateEliminationForm.getForm(formBuilder, current && current.duplicateElimination),
        });
    }
}
