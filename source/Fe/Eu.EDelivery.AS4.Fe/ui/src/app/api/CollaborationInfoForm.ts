import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ItemType } from './ItemType';
import { FormWrapper } from './../common/form.service';
import { CollaborationInfo } from './CollaborationInfo';
import { AgreementForm } from './AgreementForm';
import { ServiceForm } from './ServiceForm';

export class CollaborationInfoForm {
    public static getForm(formBuilder: FormWrapper, current: CollaborationInfo, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder.group({
            [CollaborationInfo.FIELD_action]: [formBuilder.createFieldValue(current, CollaborationInfo.FIELD_action, path, null, runtime)],
            [CollaborationInfo.FIELD_conversationId]: [formBuilder.createFieldValue(current, CollaborationInfo.FIELD_conversationId, path, null, runtime)],
            [CollaborationInfo.FIELD_agreementReference]: AgreementForm.getForm(formBuilder.subForm(CollaborationInfo.FIELD_agreementReference), current && current.agreementReference, `${path}.${CollaborationInfo.FIELD_agreementReference}`, runtime).form,
            [CollaborationInfo.FIELD_service]: ServiceForm.getForm(formBuilder.subForm(CollaborationInfo.FIELD_service), current && current.service, `${path}.${CollaborationInfo.FIELD_service}`, runtime).form,
        });
    }
}
