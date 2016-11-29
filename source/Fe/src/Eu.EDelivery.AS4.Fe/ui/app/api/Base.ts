import { FormBuilder, FormGroup } from '@angular/forms';

export class Base {
    idFormat: string;
    certificateStoreName: string;
    static getForm(formBuilder: FormBuilder, current: Base): FormGroup {
        return formBuilder.group({
            idFormat: [current && current.idFormat],
            certificateStoreName: [current && current.certificateStoreName]
        });
    }
}
