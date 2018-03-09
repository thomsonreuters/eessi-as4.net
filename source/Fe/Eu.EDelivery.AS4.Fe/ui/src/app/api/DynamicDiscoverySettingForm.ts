import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { DynamicDiscoverySetting } from './DynamicDiscoverySetting';

export class DynamicDiscoverySettingForm {
    public static getForm(formBuilder: FormBuilder, current: DynamicDiscoverySetting, isDisabled?: boolean): FormGroup {
        return formBuilder.group({
            [DynamicDiscoverySetting.FIELD_key]: [{ value: current && current.key, disabled: isDisabled }, Validators.required],
            [DynamicDiscoverySetting.FIELD_value]: [{ value: current && current.value, disabled: isDisabled }, Validators.required],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: DynamicDiscoverySetting) {
        form.removeControl(DynamicDiscoverySetting.FIELD_key);
        form.addControl(DynamicDiscoverySetting.FIELD_key, formBuilder.control(current && current.key));
        form.removeControl(DynamicDiscoverySetting.FIELD_value);
        form.addControl(DynamicDiscoverySetting.FIELD_value, formBuilder.control(current && current.value));
    }
}
