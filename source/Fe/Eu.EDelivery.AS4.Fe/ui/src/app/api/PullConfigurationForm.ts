import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PullConfiguration } from './PullConfiguration';

export class PullConfigurationForm {
    public static getForm(formBuilder: FormBuilder, current: PullConfiguration): FormGroup {
        return formBuilder.group({
            subChannel: [current && current.subChannel],
        });
    }
}
