import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PushConfiguration } from './PushConfiguration';
import { ProtocolForm } from './ProtocolForm';
import { TlsConfigurationForm } from './TlsConfigurationForm';

export class PushConfigurationForm {
    public static getForm(formBuilder: FormBuilder, current: PushConfiguration): FormGroup {
        return formBuilder.group({
            protocol: ProtocolForm.getForm(formBuilder, current && current.protocol),
            tlsConfiguration: TlsConfigurationForm.getForm(formBuilder, current && current.tlsConfiguration),
        });
    }
}
