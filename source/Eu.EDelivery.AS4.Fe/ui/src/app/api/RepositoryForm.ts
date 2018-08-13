import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Repository } from './Repository';

export class RepositoryForm {
    public static getForm(formBuilder: FormBuilder, current: Repository): FormGroup {
        return formBuilder.group({
            type: [current && current.type],
        });
    }
}
