import { FormBuilder, FormGroup, Validators } from '@angular/forms';

export class KeyEncryption {
    transportAlgorithm: string;
    digestAlgorithm: string;
    mgfAlgorithm: string;

    static FIELD_transportAlgorithm: string = 'transportAlgorithm';
    static FIELD_digestAlgorithm: string = 'digestAlgorithm';
    static FIELD_mgfAlgorithm: string = 'mgfAlgorithm';

    static getForm(formBuilder: FormBuilder, current: KeyEncryption): FormGroup {
        let form = formBuilder.group({
            [this.FIELD_transportAlgorithm]: [current && current.transportAlgorithm, Validators.required],
            [this.FIELD_digestAlgorithm]: [current && current.digestAlgorithm, Validators.required],
            [this.FIELD_mgfAlgorithm]: [current && current.mgfAlgorithm, Validators.required]
        });
        return form;
    }
    static patchForm(FormBuilder: FormBuilder, form: FormGroup, current: KeyEncryption) {
        form.get(this.FIELD_transportAlgorithm).reset({ value: current && current.transportAlgorithm, disabled: !!!current });
        form.get(this.FIELD_digestAlgorithm).reset({ value: current && current.digestAlgorithm, disabled: !!!current });
        form.get(this.FIELD_mgfAlgorithm).reset({ value: current && current.mgfAlgorithm, disabled: !!!current });
    }
}
