import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ItemType } from './ItemType';
import { PropertyForm } from './PropertyForm';

export class ItemTypeForm {
    public static getForm(formBuilder: FormBuilder, current: ItemType): FormGroup {
        return formBuilder.group({
            name: [current && current.name],
            technicalName: [current && current.technicalName],
            properties: formBuilder.array(!!!(current && current.properties) ? [] : current.properties.map(item => PropertyForm.getForm(formBuilder, item))),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ItemType) {
        form.removeControl('name');
        form.addControl('name', formBuilder.control(current && current.name));
        form.removeControl('technicalName');
        form.addControl('technicalName', formBuilder.control(current && current.technicalName));

        form.removeControl('properties');
        form.addControl('properties', formBuilder.array(!!!(current && current.properties) ? [] : current.properties.map(item => PropertyForm.getForm(formBuilder, item))));
    }
}