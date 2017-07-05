import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Security } from './Security';
import { SigningForm } from './SigningForm';
import { EncryptionForm } from './EncryptionForm';
import { FormWrapper } from './../common/form.service';

export class SecurityForm {
    public static getForm(formBuilder: FormWrapper, current: Security): FormWrapper {
        return formBuilder.group({
            signing: SigningForm.getForm(formBuilder.subForm('signing'), current && current.signing).form,
            encryption: EncryptionForm.getForm(formBuilder.subForm('encryption'), current && current.encryption).form,
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Security) {
        SigningForm.patchForm(formBuilder, <FormGroup>form.get(Security.FIELD_signing), current && current.signing);
        EncryptionForm.patchForm(formBuilder, <FormGroup>form.get(Security.FIELD_encryption), current && current.encryption);
    }
}