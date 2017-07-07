import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SigningVerification } from './SigningVerification';

export class SigningVerificationForm {
    public static getForm(formBuilder: FormBuilder, current: SigningVerification): FormGroup {
        return formBuilder.group({
            signature: [(current == null || current.signature == null) ? 0 : current.signature],
        });
    }
}
