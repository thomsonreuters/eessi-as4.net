/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
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
            id: [current && current.id],
            mep: [current && current.mep],
            mepBinding: [current && current.mepBinding],
            reliability: ReceiveReliability.getForm(formBuilder, current && current.reliability),
            receiptHandling: ReceiveReceiptHandling.getForm(formBuilder, current && current.receiptHandling),
            errorHandling: ReceiveErrorHandling.getForm(formBuilder, current && current.errorHandling),
            exceptionHandling: Receivehandling.getForm(formBuilder, current && current.exceptionHandling),
            security: ReceiveSecurity.getForm(formBuilder, current && current.security),
            messagePackaging: MessagePackaging.getForm(formBuilder, current && current.messagePackaging),
            deliver: Deliver.getForm(formBuilder, current && current.deliver),
        });
    }
    /// Patch up all the formArray controls
    static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceivingProcessingMode) {
        form.setControl('id', formBuilder.control({ value: current && current.id, disabled: !!!current }));
        form.removeControl('mep');
        form.addControl('mep', formBuilder.control(current && current.mep));
        form.removeControl('mepBinding');
        form.addControl('mepBinding', formBuilder.control(current && current.mepBinding));

        form.removeControl('reliability');
        form.addControl('reliability', ReceiveReliability.getForm(formBuilder, current && current.reliability));
        form.removeControl('receiptHandling');
        form.addControl('receiptHandling', ReceiveReceiptHandling.getForm(formBuilder, current && current.receiptHandling));
        form.removeControl('errorHandling');
        form.addControl('errorHandling', ReceiveErrorHandling.getForm(formBuilder, current && current.errorHandling));
        form.removeControl('exceptionHandling');
        form.addControl('exceptionHandling', Receivehandling.getForm(formBuilder, current && current.exceptionHandling));
        form.removeControl('security');
        form.addControl('security', ReceiveSecurity.getForm(formBuilder, current && current.security));
        form.removeControl('messagePackaging');
        form.addControl('messagePackaging', MessagePackaging.getForm(formBuilder, current && current.messagePackaging));
        form.removeControl('deliver');
        form.addControl('deliver', Deliver.getForm(formBuilder, current && current.deliver));
    }
}
