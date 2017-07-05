import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import 'rxjs/add/operator/distinctuntilchanged';

import { Encryption, PublicKeyFindCriteria, PublicKeyCertificate } from './Encryption';
import { thumbPrintValidation } from '../validators/thumbprintValidator';
import { KeyEncryptionForm } from './KeyEncryptionForm';
import { FormWrapper } from './../common/form.service';

export class EncryptionForm {
    public static getForm(formBuilder: FormWrapper, current: Encryption): FormWrapper {
        let form = formBuilder
            .group({
                isEnabled: [!!(current && current.isEnabled), Validators.required],
                algorithm: [current && current.algorithm, Validators.required],
                algorithmKeySize: [current && current.algorithmKeySize, Validators.required],
                publicKeyType: [!!!current || !!!current.publicKeyType ? 0 : current.publicKeyType],
                keyTransport: KeyEncryptionForm.getForm(formBuilder.subForm('keyTransport'), current && current.keyTransport).form,
            })
            .onChange(Encryption.FIELD_isEnabled, (result, wrapper) => {
                if (result) {
                    wrapper.enable([Encryption.FIELD_isEnabled]);
                } else {
                    wrapper.disable([Encryption.FIELD_isEnabled]);
                }
            })
            .onChange<number>(Encryption.FIELD_publicKeyType, (result, wrapper) => {
                this.setPublicKeyInfo(current, +result, wrapper);
            });

        return form;
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Encryption) {

    }
    private static getPublicKeyType(current: Encryption): number {
        if (!!current && !!current.publicKeyInformation && !!(<PublicKeyCertificate>current.publicKeyInformation).certificate) {
            return 1;
        } else if (!!current && !!current.publicKeyInformation && !!(<PublicKeyFindCriteria>current.publicKeyInformation).publicKeyFindType) {
            return 2;
        } else {
            return 0;
        }
    }
    private static setPublicKeyInfo(current: Encryption | null, type: number | number = null, formWrapper: FormWrapper): void {
        type = !!type ? type : this.getPublicKeyType(<Encryption>current);

        if (formWrapper.form.get(Encryption.FIELD_publicKeyType).value !== type) {
            if (!!!formWrapper.form.get(Encryption.FIELD_publicKeyType)) {
                formWrapper.form.setControl(Encryption.FIELD_publicKeyType, formWrapper.formBuilder.control(type));
            } else {
                formWrapper.form.get(Encryption.FIELD_publicKeyType).setValue(type);
            }
        }

        formWrapper.form.removeControl(Encryption.FIELD_publicKeyInformation);
        switch (type) {
            case 1:
                formWrapper.form.addControl(Encryption.FIELD_publicKeyInformation, formWrapper.formBuilder.group({
                    certificate: [!!!current || !!!current.publicKeyInformation ? null : (<PublicKeyCertificate>(<Encryption>current).publicKeyInformation).certificate]
                }));
                break;
            case 2:
                formWrapper.form.addControl(Encryption.FIELD_publicKeyInformation, formWrapper.formBuilder.group({
                    publicKeyFindType: [!!!current || !!!current.publicKeyInformation ? null : (<PublicKeyFindCriteria>(<Encryption>current).publicKeyInformation).publicKeyFindType],
                    publicKeyFindValue: [!!!current || !!!current.publicKeyInformation ? null : (<PublicKeyFindCriteria>(<Encryption>current).publicKeyInformation).publicKeyFindValue],
                }));
                break;
        }
    }
}
