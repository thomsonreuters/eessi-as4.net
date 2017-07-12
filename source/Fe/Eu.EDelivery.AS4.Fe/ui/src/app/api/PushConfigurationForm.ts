import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { PushConfiguration } from './PushConfiguration';
import { ProtocolForm } from './ProtocolForm';
import { TlsConfigurationForm } from './TlsConfigurationForm';

export class PushConfigurationForm {
    public static getForm(formBuilder: FormWrapper, current: PushConfiguration): FormWrapper {
        return formBuilder
            .group({
                [PushConfiguration.FIELD_protocol]: ProtocolForm.getForm(formBuilder.subForm(PushConfiguration.FIELD_protocol), current && current.protocol).form,
                [PushConfiguration.FIELD_tlsConfiguration]: TlsConfigurationForm.getForm(formBuilder.subForm(PushConfiguration.FIELD_tlsConfiguration), current && current.tlsConfiguration).form,
            });
    }
}
