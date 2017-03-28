import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReceiveReliability } from './ReceiveReliability';
import { DuplicateEliminationForm } from './DuplicateEliminationForm';

export class ReceiveReliabilityForm {
    public static getForm(formBuilder: FormBuilder, current: ReceiveReliability): FormGroup {
        return formBuilder.group({
            duplicateElimination: DuplicateEliminationForm.getForm(formBuilder, current && current.duplicateElimination),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceiveReliability) {
        form.get(ReceiveReliability.FIELD_duplicateElimination).reset({ value: current && current.duplicateElimination, disabled: !!!current && form.parent.disabled });
        DuplicateEliminationForm.patchForm(formBuilder, <FormGroup>form.get(ReceiveReliability.FIELD_duplicateElimination), current && current.duplicateElimination);
    }
}
