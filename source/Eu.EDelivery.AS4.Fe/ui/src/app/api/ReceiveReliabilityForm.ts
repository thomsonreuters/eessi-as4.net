import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { ItemType } from './ItemType';
import { ReceiveReliability } from './ReceiveReliability';
import { DuplicateEliminationForm } from './DuplicateEliminationForm';

export class ReceiveReliabilityForm {
    public static getForm(formBuilder: FormWrapper, current: ReceiveReliability, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder.group({
            duplicateElimination: DuplicateEliminationForm.getForm(formBuilder.subForm(ReceiveReliability.FIELD_duplicateElimination), current && current.duplicateElimination, `${path}.${ReceiveReliability.FIELD_duplicateElimination}`, runtime).form,
        });
    }
}
