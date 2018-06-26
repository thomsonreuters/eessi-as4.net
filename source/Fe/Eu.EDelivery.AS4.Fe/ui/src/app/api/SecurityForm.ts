import { FormWrapper } from './../common/form.service';
import { EncryptionForm } from './EncryptionForm';
import { ItemType } from './ItemType';
import { Security } from './Security';
import { SigningForm } from './SigningForm';
import { SigningVerificationForm } from './SigningVerificationForm';

export class SecurityForm {
  public static getForm(
    formBuilder: FormWrapper,
    current: Security,
    path: string,
    runtime: ItemType[]
  ): FormWrapper {
    return formBuilder.group({
      [Security.FIELD_signing]: SigningForm.getForm(
        formBuilder.subForm('signing'),
        current && current.signing,
        `${path}.${Security.FIELD_signing}`,
        runtime
      ).form,
      [Security.FIELD_signingVerification]: SigningVerificationForm.getForm(
        formBuilder.subForm(Security.FIELD_signingVerification),
        current && current.signingVerification,
        `${path}.${Security.FIELD_signingVerification}`,
        runtime
      ).form,
      [Security.FIELD_encryption]: EncryptionForm.getForm(
        formBuilder.subForm('encryption'),
        current && current.encryption,
        `${path}.${Security.FIELD_encryption}`,
        runtime
      ).form
    });
  }
}
