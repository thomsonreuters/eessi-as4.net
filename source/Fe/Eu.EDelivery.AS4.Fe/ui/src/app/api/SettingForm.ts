import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { Setting } from './Setting';

export class SettingForm {
    public static getForm(formBuilder: FormBuilder, current: Setting, valueRequired: boolean = false): FormGroup {
        return formBuilder.group({
            key: [current && current.key],
            value: [current && current.value, !valueRequired ? null : Validators.required],
            attributes: current && current.attributes && formBuilder.array(current.attributes.map((attr: Object | string) => {
                if (typeof attr === 'string') {
                    return formBuilder.group({
                        [attr]: []
                    });
                }
                let keys = Object.keys(attr);
                return formBuilder.group({
                    [keys[0]]: attr[keys[0]]
                });
            }))
        });
    }
}
