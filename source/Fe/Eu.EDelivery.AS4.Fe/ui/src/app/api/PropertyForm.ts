import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Property } from './Property';

export class PropertyForm {
    public static getForm(formBuilder: FormBuilder, current: Property): FormGroup {
        return formBuilder.group({
            friendlyName: [current && current.friendlyName],
            type: [current && current.type],
            regex: [current && current.regex],
            description: [current && current.description],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Property) {
        form.removeControl('friendlyName');
        form.addControl('friendlyName', formBuilder.control(current && current.friendlyName));
        form.removeControl('type');
        form.addControl('type', formBuilder.control(current && current.type));
        form.removeControl('regex');
        form.addControl('regex', formBuilder.control(current && current.regex));
        form.removeControl('description');
        form.addControl('description', formBuilder.control(current && current.description));
    }
}
