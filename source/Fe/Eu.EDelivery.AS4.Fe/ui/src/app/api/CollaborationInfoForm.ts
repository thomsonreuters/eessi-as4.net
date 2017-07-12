import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { CollaborationInfo } from './CollaborationInfo';
import { AgreementForm } from './AgreementForm';
import { ServiceForm } from './ServiceForm';

export class CollaborationInfoForm {
    public static getForm(formBuilder: FormWrapper, current: CollaborationInfo): FormWrapper {
        return formBuilder.group({
            [CollaborationInfo.FIELD_action]: [current && current.action],
            [CollaborationInfo.FIELD_conversationId]: [current && current.conversationId],
            [CollaborationInfo.FIELD_agreementReference]: AgreementForm.getForm(formBuilder.subForm(CollaborationInfo.FIELD_agreementReference), current && current.agreementReference).form,
            [CollaborationInfo.FIELD_service]: ServiceForm.getForm(formBuilder.subForm(CollaborationInfo.FIELD_service), current && current.service).form,
        });
    }
}
