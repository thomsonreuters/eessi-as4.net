import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReceiveSecurity } from './ReceiveSecurity';
import { SigningVerificationForm } from './SigningVerificationForm';
import { DecryptionForm } from './DecryptionForm';

export class ReceiveSecurityForm {
    public static getForm(formBuilder: FormBuilder, current: ReceiveSecurity): FormGroup {
        return formBuilder.group({
            signingVerification: SigningVerificationForm.getForm(formBuilder, current && current.signingVerification),
            decryption: DecryptionForm.getForm(formBuilder, current && current.decryption),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceiveSecurity) {
        SigningVerificationForm.patchForm(formBuilder, <FormGroup>form.get(ReceiveSecurity.FIELD_signingVerification), current && current.signingVerification);
        DecryptionForm.patchForm(formBuilder, <FormGroup>form.get(ReceiveSecurity.FIELD_decryption), current && current.decryption );
    }
}
