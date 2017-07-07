import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Protocol } from './Protocol';

export class ProtocolForm {
    public static getForm(formBuilder: FormBuilder, current: Protocol): FormGroup {
        return formBuilder.group({
            [Protocol.FIELD_url]: [current && current.url, Validators.required],
            [Protocol.FIELD_useChunking]: [!!(current && current.useChunking), Validators.required],
            [Protocol.FIELD_useHttpCompression]: [!!(current && current.useHttpCompression), Validators.required],
        });
    }
}
