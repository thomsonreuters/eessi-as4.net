import { FormWrapper } from './../common/form.service';
import { ItemType } from './ItemType';
import { ProtocolForm } from './ProtocolForm';
import { PushConfiguration } from './PushConfiguration';
import { TlsConfigurationForm } from './TlsConfigurationForm';

export class PushConfigurationForm {
  public static getForm(
    formBuilder: FormWrapper,
    current: PushConfiguration,
    runtime: ItemType[],
    path: string
  ): FormWrapper {
    return formBuilder.group({
      [PushConfiguration.FIELD_protocol]: ProtocolForm.getForm(
        formBuilder.subForm(PushConfiguration.FIELD_protocol),
        current && current.protocol,
        `${path}.${PushConfiguration.FIELD_protocol}`,
        runtime
      ).form,
      [PushConfiguration.FIELD_tlsConfiguration]: TlsConfigurationForm.getForm(
        formBuilder.subForm(PushConfiguration.FIELD_tlsConfiguration),
        current && current.tlsConfiguration,
        runtime,
        `${path}.${PushConfiguration.FIELD_tlsConfiguration}`
      ).form
    });
  }
}
