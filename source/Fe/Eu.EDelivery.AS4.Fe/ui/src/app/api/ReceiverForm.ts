import { Injector } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { RuntimeStore } from './../settings/runtime.store';
import { FormWrapper } from './../common/form.service';
import { Receiver } from './Receiver';
import { SettingForm } from './SettingForm';

export class ReceiverForm {
    public static getForm(formBuilder: FormWrapper, current: Receiver | undefined, injector: Injector): FormWrapper {
        let receiver = !!!current ? null : injector.get<RuntimeStore>(RuntimeStore).getState().receivers.find((receiver) => receiver.technicalName === (current && current.type));
        return formBuilder
            .group({
                type: [current && current.type],
                setting: formBuilder.formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => {
                    let isRequired = receiver!.properties.find((prop) => prop.technicalName === item.key);
                    return SettingForm.getForm(formBuilder.formBuilder, item, isRequired!.required);
                })),
            });
    }
}
