import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PullConfiguration } from './PullConfiguration';

export class PullConfigurationForm {
    public static getForm(formBuilder: FormBuilder, current: PullConfiguration): FormGroup {
        return formBuilder.group({
            subChannel: [current && current.subChannel],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: PullConfiguration) {
        form.get(PullConfiguration.FIELD_subChannel).reset({ value: current && current.subChannel, disabled: !!!current });
    }
}
