import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { Steps } from './Steps';
import { StepForm } from './StepForm';

export class StepsForm {
    public static getForm(formBuilder: FormWrapper, current: Steps | undefined): FormWrapper {
        return formBuilder.group({
            normalPipeline: formBuilder.formBuilder.array(!!!(current && current.normalPipeline) ? [] : current.normalPipeline.map(item => StepForm.getForm(formBuilder.formBuilder, item))),
            errorPipeline: formBuilder.formBuilder.array(!!!(current && current.errorPipeline) ? [] : current.errorPipeline.map(item => StepForm.getForm(formBuilder.formBuilder, item))),
        });
    }
}
