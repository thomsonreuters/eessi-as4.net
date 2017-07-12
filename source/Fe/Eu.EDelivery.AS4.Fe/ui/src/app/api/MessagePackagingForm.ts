import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { MessagePackaging } from './MessagePackaging';
import { PartyInfoForm } from './PartyInfoForm';
import { CollaborationInfoForm } from './CollaborationInfoForm';
import { MessagePropertyForm } from './MessagePropertyForm';

export class MessagePackagingForm {
    public static getForm(formBuilder: FormWrapper, current: MessagePackaging): FormWrapper {
        return formBuilder.group({
            partyInfo: PartyInfoForm.getForm(formBuilder.formBuilder, current && current.partyInfo),
            collaborationInfo: CollaborationInfoForm.getForm(formBuilder.subForm(MessagePackaging.FIELD_collaborationInfo), current && current.collaborationInfo).form,
            messageProperties: formBuilder.formBuilder.array(!!!(current && current.messageProperties) ? [] : current.messageProperties.map(item => MessagePropertyForm.getForm(formBuilder.formBuilder, item))),
        });
    }
}
