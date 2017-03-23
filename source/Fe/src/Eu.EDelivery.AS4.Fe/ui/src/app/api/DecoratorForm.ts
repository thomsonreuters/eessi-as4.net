import { Decorator } from './Decorator';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { StepsForm } from './StepsForm';

export class DecoratorForm {
    public static getForm(formBuilder: FormBuilder, current: Decorator): FormGroup {
        return formBuilder.group({
            type: [current && current.type],
            steps: StepsForm.getForm(formBuilder, current && current.steps),
        });
    }
}
