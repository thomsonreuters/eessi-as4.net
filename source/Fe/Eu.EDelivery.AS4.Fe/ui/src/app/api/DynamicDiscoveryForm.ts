import { Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { DynamicDiscovery } from './DynamicDiscovery';
import { DynamicDiscoverySettingForm } from './DynamicDiscoverySettingForm';
import { ItemType } from './ItemType';

/* tslint:disable */
export class DynamicDiscoveryForm {
    public static getForm(formBuilder: FormWrapper, current: DynamicDiscovery | null, runtime: ItemType[], path: string): FormWrapper {
        return formBuilder
            .group({
                [DynamicDiscovery.FIELD_smpProfile]: [formBuilder.createFieldValue(current, DynamicDiscovery.FIELD_smpProfile, path, null, runtime), Validators.required],
                [DynamicDiscovery.FIELD_settings]: formBuilder.formBuilder.array(!!!(current && current.settings) ? [] : current.settings.map(item => DynamicDiscoverySettingForm.getForm(formBuilder.formBuilder, item)))})
            };
}
