import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Protocol } from './Protocol';
import { FormWrapper } from './../common/form.service';

export class ProtocolForm {
    public static getForm(formBuilder: FormWrapper, current: Protocol): FormWrapper {
        return formBuilder
            .group({
                [Protocol.FIELD_url]: [current && current.url, Validators.required],
                [Protocol.FIELD_useChunking]: [!!(current && current.useChunking), Validators.required],
                [Protocol.FIELD_useHttpCompression]: [!!(current && current.useHttpCompression), Validators.required],
            })
            .onStatusChange(undefined, (status, wrapper) => {
                if (status === 'INVALID') {
                    if (!!!wrapper.form.get(Protocol.FIELD_useChunking)!.value) {
                        !!!wrapper.form.get(Protocol.FIELD_useChunking)!.setValue(false);
                    }
                    if (!!!wrapper.form.get(Protocol.FIELD_useHttpCompression)!.value) {
                        !!!wrapper.form.get(Protocol.FIELD_useHttpCompression)!.setValue(false) ;
                    }
                }
            });
    }
}
