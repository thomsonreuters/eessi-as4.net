/* tslint:disable */
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ReceiveReliability } from "./ReceiveReliability";
import { ReceiveReceiptHandling } from "./ReceiveReceiptHandling";
import { ReceiveErrorHandling } from "./ReceiveErrorHandling";
import { Receivehandling } from "./Receivehandling";
import { ReceiveSecurity } from "./ReceiveSecurity";
import { MessagePackaging } from "./MessagePackaging";
import { Deliver } from "./Deliver";
import { IPmode } from './Pmode.interface';

export class ReceivingProcessingMode {
    id: string;
    mep: number;
    mepBinding: number;
    reliability: ReceiveReliability = new ReceiveReliability();
    receiptHandling: ReceiveReceiptHandling = new ReceiveReceiptHandling();
    errorHandling: ReceiveErrorHandling = new ReceiveErrorHandling();
    exceptionHandling: Receivehandling = new Receivehandling();
    security: ReceiveSecurity = new ReceiveSecurity();
    messagePackaging: MessagePackaging = new MessagePackaging();
    deliver: Deliver = new Deliver();

    static FIELD_id: string = 'id';
    static FIELD_mep: string = 'mep';
    static FIELD_mepBinding: string = 'mepBinding';
    static FIELD_reliability: string = 'reliability';
    static FIELD_receiptHandling: string = 'receiptHandling';
    static FIELD_errorHandling: string = 'errorHandling';
    static FIELD_exceptionHandling: string = 'exceptionHandling';
    static FIELD_security: string = 'security';
    static FIELD_messagePackaging: string = 'messagePackaging';
    static FIELD_deliver: string = 'deliver';

    static getForm(formBuilder: FormBuilder, current: ReceivingProcessingMode): FormGroup {
        return formBuilder.group({
            [this.FIELD_id]: [current && current.id, Validators.required],
            [this.FIELD_mep]: [(current == null || current.mep == null) ? 0 : current.mep, Validators.required],
            [this.FIELD_mepBinding]: [(current == null || current.mepBinding == null) ? 1 : current.mepBinding, Validators.required],
            [this.FIELD_reliability]: ReceiveReliability.getForm(formBuilder, current && current.reliability),
            [this.FIELD_receiptHandling]: ReceiveReceiptHandling.getForm(formBuilder, current && current.receiptHandling),
            [this.FIELD_errorHandling]: ReceiveErrorHandling.getForm(formBuilder, current && current.errorHandling),
            [this.FIELD_exceptionHandling]: Receivehandling.getForm(formBuilder, current && current.exceptionHandling),
            [this.FIELD_security]: ReceiveSecurity.getForm(formBuilder, current && current.security),
            [this.FIELD_messagePackaging]: MessagePackaging.getForm(formBuilder, current && current.messagePackaging),
            [this.FIELD_deliver]: Deliver.getForm(formBuilder, current && current.deliver),
        });
    }
    /// Patch up all the formArray controls
    static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceivingProcessingMode) {
        form.get(this.FIELD_id).reset({ value: current && current.id, disabled: !!!current || form.parent.disabled });
        form.get(this.FIELD_mep).reset({ value: current && current.mep, disabled: !!!current || form.parent.disabled });
        form.get(this.FIELD_mepBinding).reset({ value: current && current.mepBinding, disabled: !!!current || form.parent.disabled });
        ReceiveReliability.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_reliability), current && current.reliability);
        ReceiveReceiptHandling.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_receiptHandling), current && current.receiptHandling);
        ReceiveErrorHandling.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_errorHandling), current && current.errorHandling);
        Receivehandling.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_exceptionHandling), current && current.exceptionHandling);
        ReceiveSecurity.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_security), current && current.security);
        MessagePackaging.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_messagePackaging), current && current.messagePackaging);
        Deliver.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_deliver), current && current.deliver);
    }
}
