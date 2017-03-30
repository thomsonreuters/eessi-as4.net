import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SigningVerification } from './SigningVerification';

export class SigningVerificationForm {
    public static getForm(formBuilder: FormBuilder, current: SigningVerification): FormGroup {
        return formBuilder.group({
            signature: [(current == null || current.signature == null) ? 0 : current.signature],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SigningVerification) {
        form.get(SigningVerification.FIELD_signature).reset({ value: current && current.signature, disabled: !!!current });
    }
}
