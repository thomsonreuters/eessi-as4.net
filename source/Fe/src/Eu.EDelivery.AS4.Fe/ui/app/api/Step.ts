import { Setting } from './Setting';
/* tslint:disable */
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

export class Step {
    type: string;
    unDecorated: boolean;
    setting: Array<Setting>;

    static FIELD_type: string = 'type';
    static FIELD_unDecorated: string = 'unDecorated';
    static FIELD_setting: string = 'setting';

    static getForm(formBuilder: FormBuilder, current: Step): FormGroup {
        return formBuilder.group({
            type: [current && current.type, Validators.required],
            unDecorated: [!!(current && current.unDecorated), Validators.required],
            setting: formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => Setting.getForm(formBuilder, item)))
        });
    }
}
