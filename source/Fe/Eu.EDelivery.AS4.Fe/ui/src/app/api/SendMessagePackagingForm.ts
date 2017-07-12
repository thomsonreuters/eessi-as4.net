import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { SendMessagePackaging } from './SendMessagePackaging';
import { PartyInfoForm } from './PartyInfoForm';
import { CollaborationInfoForm } from './CollaborationInfoForm';
import { MessagePropertyForm } from './MessagePropertyForm';

export class SendMessagePackagingForm {
    public static getForm(formBuilder: FormWrapper, current: SendMessagePackaging): FormWrapper {
        return formBuilder.group({
            mpc: [current && current.mpc],
            useAS4Compression: [!!(current && current.useAS4Compression)],
            isMultiHop: [!!(current && current.isMultiHop)],
            includePModeId: [!!(current && current.includePModeId)],
            partyInfo: PartyInfoForm.getForm(formBuilder.formBuilder, current && current.partyInfo),
            collaborationInfo: CollaborationInfoForm.getForm(formBuilder.subForm(SendMessagePackaging.FIELD_collaborationInfo), current && current.collaborationInfo).form,
            messageProperties: formBuilder.formBuilder.array(!!!(current && current.messageProperties) ? [] : current.messageProperties.map(item => MessagePropertyForm.getForm(formBuilder.formBuilder, item))),
        });
    }
}
