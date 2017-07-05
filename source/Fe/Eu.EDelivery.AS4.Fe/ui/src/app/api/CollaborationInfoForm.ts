import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CollaborationInfo } from './CollaborationInfo';
import { AgreementForm } from './AgreementForm';
import { ServiceForm } from './ServiceForm';

export class CollaborationInfoForm {
    public static getForm(formBuilder: FormBuilder, current: CollaborationInfo): FormGroup {
        return formBuilder.group({
            [CollaborationInfo.FIELD_action]: [current && current.action],
            [CollaborationInfo.FIELD_conversationId]: [current && current.conversationId],
            [CollaborationInfo.FIELD_agreementReference]: AgreementForm.getForm(formBuilder, current && current.agreementReference),
            [CollaborationInfo.FIELD_service]: ServiceForm.getForm(formBuilder, current && current.service),
        });
    }
}
