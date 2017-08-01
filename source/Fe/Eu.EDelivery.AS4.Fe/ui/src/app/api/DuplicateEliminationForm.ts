import { FormWrapper } from './../common/form.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ItemType } from './ItemType';
import { DuplicateElimination } from './DuplicateElimination';

export class DuplicateEliminationForm {
    public static getForm(formBuilder: FormWrapper, current: DuplicateElimination, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder.group({
            isEnabled: [formBuilder.createFieldValue(current, DuplicateElimination.FIELD_isEnabled, path, false, runtime)],
        });
    }
}
