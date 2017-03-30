import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Security } from './Security';
import { SigningForm } from './SigningForm';
import { EncryptionForm } from './EncryptionForm';

export class SecurityForm {
    public static getForm(formBuilder: FormBuilder, current: Security): FormGroup {
        return formBuilder.group({
            signing: SigningForm.getForm(formBuilder, current && current.signing),
            encryption: EncryptionForm.getForm(formBuilder, current && current.encryption),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Security) {
        SigningForm.patchForm(formBuilder, <FormGroup>form.get(Security.FIELD_signing), current && current.signing);
        EncryptionForm.patchForm(formBuilder, <FormGroup>form.get(Security.FIELD_encryption), current && current.encryption);
    }
}