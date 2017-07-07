import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { ReceiveSecurity } from './ReceiveSecurity';
import { SigningVerificationForm } from './SigningVerificationForm';
import { DecryptionForm } from './DecryptionForm';

export class ReceiveSecurityForm {
    public static getForm(formBuilder: FormWrapper, current: ReceiveSecurity): FormWrapper {
        return formBuilder.group({
            signingVerification: SigningVerificationForm.getForm(formBuilder.formBuilder, current && current.signingVerification),
            decryption: DecryptionForm.getForm(formBuilder.subForm(ReceiveSecurity.FIELD_decryption), current && current.decryption).form,
        });
    }
}
