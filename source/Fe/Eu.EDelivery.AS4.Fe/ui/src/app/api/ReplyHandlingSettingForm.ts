import { ReceiveReceiptHandlingForm } from './ReceiveReceiptHandlingForm';
import { ReceiveErrorHandlingForm } from './ReceiveErrorHandlingForm';
import { ReplyHandlingSetting } from './ReplyHandlingSetting';
import { FormWrapper } from './../common/form.service';

export class ReplyHandlingSettingForm {
    public static getForm(formBuilder: FormWrapper, current: ReplyHandlingSetting): FormWrapper {
        return formBuilder
            .group({
                [ReplyHandlingSetting.FIELD_replyPattern]: [current && current.replyPattern],
                [ReplyHandlingSetting.FIELD_sendingPmode]: [current && current.sendingPMode],
                [ReplyHandlingSetting.FIELD_receiptHandling]: ReceiveReceiptHandlingForm.getForm(formBuilder.subForm(ReplyHandlingSetting.FIELD_receiptHandling), current && current.receiptHandling).form,
                [ReplyHandlingSetting.FIELD_receiveErrorHandling]: ReceiveErrorHandlingForm.getForm(formBuilder.subForm(ReplyHandlingSetting.FIELD_receiveErrorHandling), current && current.errorHandling).form
            });
    }
}
