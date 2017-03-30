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
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Protocol) {
        form.get(Protocol.FIELD_url).reset({ value: current && current.url, disabled: !!!current });
        form.get(Protocol.FIELD_useChunking).reset({ value: current && current.useChunking, disabled: !!!current });
        form.get(Protocol.FIELD_useHttpCompression).reset({ value: current && current.useHttpCompression, disabled: !!!current });
    }
}
