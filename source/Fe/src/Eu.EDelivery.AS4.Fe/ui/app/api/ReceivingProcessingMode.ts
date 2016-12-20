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
    reliability: ReceiveReliability;
    receiptHandling: ReceiveReceiptHandling;
    errorHandling: ReceiveErrorHandling;
    exceptionHandling: Receivehandling;
    security: ReceiveSecurity;
    messagePackaging: MessagePackaging;
    deliver: Deliver;

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
            [this.FIELD_mep]: [current && current.mep, Validators.required],
            [this.FIELD_mepBinding]: [current && current.mepBinding, Validators.required],
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
        form.setControl(this.FIELD_id, formBuilder.control({ value: current && current.id, disabled: !!!current }));
        form.removeControl(this.FIELD_mep);
        form.addControl(this.FIELD_mep, formBuilder.control(current && current.mep));
        form.removeControl(this.FIELD_mepBinding);
        form.addControl(this.FIELD_mepBinding, formBuilder.control(current && current.mepBinding));

        form.removeControl(this.FIELD_reliability);
        form.addControl(this.FIELD_reliability, ReceiveReliability.getForm(formBuilder, current && current.reliability));
        form.removeControl(this.FIELD_receiptHandling);
        form.addControl(this.FIELD_receiptHandling, ReceiveReceiptHandling.getForm(formBuilder, current && current.receiptHandling));
        form.removeControl(this.FIELD_errorHandling);
        form.addControl(this.FIELD_errorHandling, ReceiveErrorHandling.getForm(formBuilder, current && current.errorHandling));
        form.removeControl(this.FIELD_exceptionHandling);
        form.addControl(this.FIELD_exceptionHandling, Receivehandling.getForm(formBuilder, current && current.exceptionHandling));
        form.removeControl(this.FIELD_security);
        form.addControl(this.FIELD_security, ReceiveSecurity.getForm(formBuilder, current && current.security));
        form.removeControl(this.FIELD_messagePackaging);
        form.addControl(this.FIELD_messagePackaging, MessagePackaging.getForm(formBuilder, current && current.messagePackaging));
        form.removeControl(this.FIELD_deliver);
        form.addControl(this.FIELD_deliver, Deliver.getForm(formBuilder, current && current.deliver));
    }
}
