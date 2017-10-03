import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { ItemType } from './ItemType';
import { SendMessagePackaging } from './SendMessagePackaging';
import { PartyInfoForm } from './PartyInfoForm';
import { CollaborationInfoForm } from './CollaborationInfoForm';
import { MessagePropertyForm } from './MessagePropertyForm';

export class SendMessagePackagingForm {
    public static getForm(formBuilder: FormWrapper, current: SendMessagePackaging, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder.group({
            [SendMessagePackaging.FIELD_mpc]: [formBuilder.createFieldValue(current, SendMessagePackaging.FIELD_mpc, path, null, runtime)],
            [SendMessagePackaging.FIELD_useAS4Compression]: [formBuilder.createFieldValue(current, SendMessagePackaging.FIELD_useAS4Compression, path, true, runtime)],
            [SendMessagePackaging.FIELD_isMultiHop]: [formBuilder.createFieldValue(current, SendMessagePackaging.FIELD_isMultiHop, path, null, runtime)],
            [SendMessagePackaging.FIELD_includePModeId]: [formBuilder.createFieldValue(current, SendMessagePackaging.FIELD_includePModeId, path, null, runtime)],
            [SendMessagePackaging.FIELD_partyInfo]: PartyInfoForm.getForm(formBuilder.subForm(SendMessagePackaging.FIELD_partyInfo), current && current.partyInfo, `${path}.${SendMessagePackaging.FIELD_partyInfo}`, runtime).form,
            [SendMessagePackaging.FIELD_collaborationInfo]: CollaborationInfoForm.getForm(formBuilder.subForm(SendMessagePackaging.FIELD_collaborationInfo), current && current.collaborationInfo, `${path}.${SendMessagePackaging.FIELD_collaborationInfo}`, runtime).form,
            [SendMessagePackaging.FIELD_messageProperties]: formBuilder.formBuilder.array(!!!(current && current.messageProperties) ? [] : current.messageProperties.map(item => MessagePropertyForm.getForm(formBuilder.formBuilder, item))),
        });
    }
}
