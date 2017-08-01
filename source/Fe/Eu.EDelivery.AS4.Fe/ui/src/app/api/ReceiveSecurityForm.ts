import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { ItemType } from './ItemType';
import { ReceiveSecurity } from './ReceiveSecurity';
import { SigningVerificationForm } from './SigningVerificationForm';
import { DecryptionForm } from './DecryptionForm';

export class ReceiveSecurityForm {
    public static getForm(formBuilder: FormWrapper, current: ReceiveSecurity, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder.group({
            signingVerification: SigningVerificationForm.getForm(formBuilder.subForm(ReceiveSecurity.FIELD_signingVerification), current && current.signingVerification, `${path}.${ReceiveSecurity.FIELD_signingVerification}`, runtime).form,
            decryption: DecryptionForm.getForm(formBuilder.subForm(ReceiveSecurity.FIELD_decryption), current && current.decryption, `${path}.${ReceiveSecurity.FIELD_decryption}`, runtime).form,
        });
    }
}
