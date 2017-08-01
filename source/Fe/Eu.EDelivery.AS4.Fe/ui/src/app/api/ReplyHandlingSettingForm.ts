import { ItemType } from './ItemType';
import { ReceiveReceiptHandlingForm } from './ReceiveReceiptHandlingForm';
import { ReceiveErrorHandlingForm } from './ReceiveErrorHandlingForm';
import { ReplyHandlingSetting } from './ReplyHandlingSetting';
import { FormWrapper } from './../common/form.service';

export class ReplyHandlingSettingForm {
    public static getForm(formBuilder: FormWrapper, current: ReplyHandlingSetting, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder
            .group({
                [ReplyHandlingSetting.FIELD_replyPattern]: [formBuilder.createFieldValue(current, ReplyHandlingSetting.FIELD_replyPattern, path, null, runtime)],
                [ReplyHandlingSetting.FIELD_sendingPmode]: [formBuilder.createFieldValue(current, ReplyHandlingSetting.FIELD_sendingPmode, path, null, runtime)],
                [ReplyHandlingSetting.FIELD_receiptHandling]: ReceiveReceiptHandlingForm.getForm(formBuilder.subForm(ReplyHandlingSetting.FIELD_receiptHandling), current && current.receiptHandling, `${path}.${ReplyHandlingSetting.FIELD_receiptHandling}`, runtime).form,
                [ReplyHandlingSetting.FIELD_receiveErrorHandling]: ReceiveErrorHandlingForm.getForm(formBuilder.subForm(ReplyHandlingSetting.FIELD_receiveErrorHandling), current && current.errorHandling, `${path}.${ReplyHandlingSetting.FIELD_receiveErrorHandling}`, runtime).form
            });
    }
}
