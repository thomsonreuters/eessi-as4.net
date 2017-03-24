import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PartyId } from './PartyId';

export class PartyIdForm {
    public static getForm(formBuilder: FormBuilder, current: PartyId): FormGroup {
        return formBuilder.group({
            id: [current && current.id],
            type: [current && current.type],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: PartyId) {
        form.removeControl('id');
        form.addControl('id', formBuilder.control(current && current.id));
        form.removeControl('type');
        form.addControl('type', formBuilder.control(current && current.type));

    }
}
