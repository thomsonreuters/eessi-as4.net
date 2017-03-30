import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Steps } from './Steps';
import { StepForm } from './StepForm';

export class StepsForm {
    public static getForm(formBuilder: FormBuilder, current: Steps): FormGroup {
        return formBuilder.group({
            decorator: [current && current.decorator],
            step: formBuilder.array(!!!(current && current.step) ? [] : current.step.map(item => StepForm.getForm(formBuilder, item))),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Steps) {
        form.removeControl('decorator');
        form.addControl('decorator', formBuilder.control(current && current.decorator));

        form.removeControl('step');
        form.addControl('step', formBuilder.array(!!!(current && current.step) ? [] : current.step.map(item => StepForm.getForm(formBuilder, item))));
    }
}
