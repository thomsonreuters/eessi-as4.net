import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Repository } from './Repository';

export class RepositoryForm {
    public static getForm(formBuilder: FormBuilder, current: Repository): FormGroup {
        return formBuilder.group({
            type: [current && current.type],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Repository) {
        form.get(Repository.FIELD_type).reset({ value: current && current.type });
    }
}
