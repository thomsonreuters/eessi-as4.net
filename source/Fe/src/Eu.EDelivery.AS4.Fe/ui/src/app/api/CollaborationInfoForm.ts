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
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: CollaborationInfo) {
        form.get(CollaborationInfo.FIELD_action).reset({ value: current && current.action, disabled: !!!current && form.parent.disabled });
        form.get(CollaborationInfo.FIELD_conversationId).reset({ value: current && current.conversationId, disabled: !!!current && form.parent.disabled });
        AgreementForm.patchForm(formBuilder, <FormGroup>form.get(CollaborationInfo.FIELD_agreementReference), current && current.agreementReference);
        ServiceForm.patchForm(formBuilder, <FormGroup>form.get(CollaborationInfo.FIELD_service), current && current.service);
    }
}