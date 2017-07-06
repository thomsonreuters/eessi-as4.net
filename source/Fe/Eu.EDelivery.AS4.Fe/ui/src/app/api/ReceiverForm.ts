import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { Receiver } from './Receiver';
import { SettingForm } from './SettingForm';

export class ReceiverForm {
    public static getForm(formBuilder: FormWrapper, current: Receiver): FormWrapper {
        return formBuilder
            .group({
                type: [current && current.type],
                setting: formBuilder.formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => SettingForm.getForm(formBuilder.formBuilder, item))),
            });
    }
}
