import { FormWrapper } from './../common/form.service';
import { ItemType } from './ItemType';
import { ReceiveErrorHandlingForm } from './ReceiveErrorHandlingForm';
import { ReceiveReceiptHandlingForm } from './ReceiveReceiptHandlingForm';
import { ReplyHandlingSetting } from './ReplyHandlingSetting';
import { RetryReliabilityForm } from './RetryReliabilityForm';

export class ReplyHandlingSettingForm {
  public static getForm(
    formBuilder: FormWrapper,
    current: ReplyHandlingSetting,
    path: string,
    runtime: ItemType[]
  ): FormWrapper {
    return formBuilder.group({
      [ReplyHandlingSetting.FIELD_replyPattern]: [
        formBuilder.createFieldValue(
          current,
          ReplyHandlingSetting.FIELD_replyPattern,
          path,
          0,
          runtime
        )
      ],
      [ReplyHandlingSetting.FIELD_receiptHandling]: ReceiveReceiptHandlingForm.getForm(
        formBuilder.subForm(ReplyHandlingSetting.FIELD_receiptHandling),
        current && current.receiptHandling,
        `${path}.${ReplyHandlingSetting.FIELD_receiptHandling}`,
        runtime
      ).form,
      [ReplyHandlingSetting.FIELD_receiveErrorHandling]: ReceiveErrorHandlingForm.getForm(
        formBuilder.subForm(ReplyHandlingSetting.FIELD_receiveErrorHandling),
        current && current.errorHandling,
        `${path}.${ReplyHandlingSetting.FIELD_receiveErrorHandling}`,
        runtime
      ).form,
      [ReplyHandlingSetting.FIELD_piggyBackReliability]: RetryReliabilityForm.getForm(
        formBuilder.subForm(ReplyHandlingSetting.FIELD_piggyBackReliability),
        current && current.piggyBackReliability,
        `${path}.${ReplyHandlingSetting.FIELD_piggyBackReliability}`,
        runtime
      ).form
    });
  }
}
