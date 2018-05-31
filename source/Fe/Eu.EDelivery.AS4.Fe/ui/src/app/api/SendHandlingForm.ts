import { FormWrapper } from '../common/form.service';
import { ItemType } from './ItemType';
import { MethodForm } from './MethodForm';
import { RetryReliabilityForm } from './RetryReliabilityForm';
import { SendHandling } from './SendHandling';

export class SendHandlingForm {
  public static getForm(
    formBuilder: FormWrapper,
    current: SendHandling,
    path: string,
    runtime: ItemType[]
  ): FormWrapper {
    let form = formBuilder
      .group({
        [SendHandling.FIELD_notifyMessageProducer]: [
          formBuilder.createFieldValue(
            current,
            SendHandling.FIELD_notifyMessageProducer,
            path,
            null,
            runtime
          )
        ],
        [SendHandling.FIELD_notifyMethod]: MethodForm.getForm(
          formBuilder.subForm(SendHandling.FIELD_notifyMethod),
          current && current.notifyMethod,
          `${path}.${SendHandling.FIELD_notifyMethod}`,
          runtime
        ).form,
        [SendHandling.FIELD_reliability]: RetryReliabilityForm.getForm(
          formBuilder.subForm(SendHandling.FIELD_reliability),
          current && current.reliability,
          `${path}.${SendHandling.FIELD_reliability}`,
          runtime
        ).form
      })
      .onChange<boolean>(
        SendHandling.FIELD_notifyMessageProducer,
        (value, wrapper) => {
          if (!!value) {
            wrapper.enable([SendHandling.FIELD_notifyMessageProducer]);
          } else {
            wrapper.disable([SendHandling.FIELD_notifyMessageProducer]);
          }
        }
      )
      .triggerHandler(
        SendHandling.FIELD_notifyMessageProducer,
        current && current.notifyMessageProducer
      );
    return form;
  }
}
