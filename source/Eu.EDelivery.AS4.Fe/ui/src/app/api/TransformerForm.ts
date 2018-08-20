import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { Transformer } from './Transformer';
import { Injector } from '@angular/core';
import { RuntimeStore } from '../settings/runtime.store';
import { SettingForm } from './SettingForm';

export class TransformerForm {
    public static getForm(formBuilder: FormWrapper, current: Transformer | undefined, injector: Injector): FormWrapper {
        let transformer = !!!current ? null : injector.get<RuntimeStore>(RuntimeStore).getState().transformers.find((transformer) => transformer.technicalName === (current && current.type));
        return formBuilder
            .group({
                type: [current && current.type, Validators.required],
                setting: formBuilder.formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => {
                    let isRequired = !!!transformer ? {required: false} : transformer.properties.find((prop) => prop.technicalName === item.key);
                    return SettingForm.getForm(formBuilder.formBuilder, item, isRequired!.required);
                })),
            });
    }
}
