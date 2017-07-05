import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessagePackaging } from './MessagePackaging';
import { PartyInfoForm } from './PartyInfoForm';
import { CollaborationInfoForm } from './CollaborationInfoForm';
import { MessagePropertyForm } from './MessagePropertyForm';

export class MessagePackagingForm {
    public static getForm(formBuilder: FormBuilder, current: MessagePackaging): FormGroup {
        return formBuilder.group({
            partyInfo: PartyInfoForm.getForm(formBuilder, current && current.partyInfo),
            collaborationInfo: CollaborationInfoForm.getForm(formBuilder, current && current.collaborationInfo),
            messageProperties: formBuilder.array(!!!(current && current.messageProperties) ? [] : current.messageProperties.map(item => MessagePropertyForm.getForm(formBuilder, item))),
        });
    }
}
