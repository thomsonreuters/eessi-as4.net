import { FormWrapper } from './../common/form.service';
import { ItemType } from './ItemType';
import { SigningVerification } from './SigningVerification';

export class SigningVerificationForm {
    public static getForm(formBuilder: FormWrapper, current: SigningVerification, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder.group({
            signature: [formBuilder.createFieldValue(current, SigningVerification.FIELD_signature, path, null, runtime)],
        });
    }
}
