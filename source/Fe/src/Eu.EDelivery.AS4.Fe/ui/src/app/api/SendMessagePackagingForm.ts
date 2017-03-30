import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SendMessagePackaging } from './SendMessagePackaging';
import { PartyInfoForm } from './PartyInfoForm';
import { CollaborationInfoForm } from './CollaborationInfoForm';
import { MessagePropertyForm } from './MessagePropertyForm';

export class SendMessagePackagingForm {
    public static getForm(formBuilder: FormBuilder, current: SendMessagePackaging): FormGroup {
        return formBuilder.group({
            mpc: [current && current.mpc],
            useAS4Compression: [!!(current && current.useAS4Compression)],
            isMultiHop: [!!(current && current.isMultiHop)],
            includePModeId: [!!(current && current.includePModeId)],
            partyInfo: PartyInfoForm.getForm(formBuilder, current && current.partyInfo),
            collaborationInfo: CollaborationInfoForm.getForm(formBuilder, current && current.collaborationInfo),
            messageProperties: formBuilder.array(!!!(current && current.messageProperties) ? [] : current.messageProperties.map(item => MessagePropertyForm.getForm(formBuilder, item))),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SendMessagePackaging) {
        form.removeControl('mpc');
        form.addControl('mpc', formBuilder.control(current && current.mpc));
        form.removeControl('useAS4Compression');
        form.addControl('useAS4Compression', formBuilder.control(current && current.useAS4Compression));
        form.removeControl('isMultiHop');
        form.addControl('isMultiHop', formBuilder.control(current && current.isMultiHop));
        form.removeControl('includePModeId');
        form.addControl('includePModeId', formBuilder.control(current && current.includePModeId));

        form.removeControl('partyInfo');
        form.addControl('partyInfo', PartyInfoForm.getForm(formBuilder, current && current.partyInfo));
        form.removeControl('collaborationInfo');
        form.addControl('collaborationInfo', CollaborationInfoForm.getForm(formBuilder, current && current.collaborationInfo));
        form.removeControl('messageProperties');
        form.addControl('messageProperties', formBuilder.array(!!!(current && current.messageProperties) ? [] : current.messageProperties.map(item => MessagePropertyForm.getForm(formBuilder, item))));
    }
}