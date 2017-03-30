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
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: MessagePackaging) {
        form.get(MessagePackaging.FIELD_partyInfo).reset({ value: current && current.partyInfo, disabled: !!!current && form.parent.disabled });
        PartyInfoForm.patchForm(formBuilder, <FormGroup>form.get(MessagePackaging.FIELD_partyInfo), current && current.partyInfo);
        CollaborationInfoForm.patchForm(formBuilder, <FormGroup>form.get(MessagePackaging.FIELD_collaborationInfo), current && current.collaborationInfo);
        form.setControl(MessagePackaging.FIELD_messageProperties, formBuilder.array(!!!(current && current.messageProperties) ? [] : current.messageProperties.map(item => MessagePropertyForm.getForm(formBuilder, item))));
    }
}
