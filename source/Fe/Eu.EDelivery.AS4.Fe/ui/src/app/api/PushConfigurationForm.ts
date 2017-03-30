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
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: PushConfiguration) {
        ProtocolForm.patchForm(formBuilder, <FormGroup>form.get(PushConfiguration.FIELD_protocol), current && current.protocol);
        TlsConfigurationForm.patchForm(formBuilder, <FormGroup>form.get(PushConfiguration.FIELD_tlsConfiguration), current && current.tlsConfiguration);
    }
}
