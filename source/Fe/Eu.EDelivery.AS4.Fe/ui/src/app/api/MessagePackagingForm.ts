import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ItemType } from './ItemType';
import { FormWrapper } from './../common/form.service';
import { MessagePackaging } from './MessagePackaging';
import { PartyInfoForm } from './PartyInfoForm';
import { CollaborationInfoForm } from './CollaborationInfoForm';
import { MessagePropertyForm } from './MessagePropertyForm';

export class MessagePackagingForm {
    public static getForm(formBuilder: FormWrapper, current: MessagePackaging, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder.group({
            partyInfo: PartyInfoForm.getForm(formBuilder.subForm(MessagePackaging.FIELD_partyInfo), current && current.partyInfo, `${path}.${MessagePackaging.FIELD_partyInfo}`, runtime).form,
            collaborationInfo: CollaborationInfoForm.getForm(formBuilder.subForm(MessagePackaging.FIELD_collaborationInfo), current && current.collaborationInfo, `${path}.${MessagePackaging.FIELD_collaborationInfo}`, runtime).form,
            messageProperties: formBuilder.formBuilder.array(!!!(current && current.messageProperties) ? [] : current.messageProperties.map(item => MessagePropertyForm.getForm(formBuilder.formBuilder, item))),
        });
    }
}
