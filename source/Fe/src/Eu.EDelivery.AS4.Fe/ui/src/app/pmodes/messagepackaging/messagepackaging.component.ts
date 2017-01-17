import { FormGroup } from '@angular/forms';
import { Component, Input } from '@angular/core';

@Component({
    selector: 'as4-message-packaging',
    template: `
        <div [formGroup]="form">
            <as4-party label="From party" [group]="form.get('partyInfo.fromParty')" runtimeTooltip="partyinfo.fromparty"></as4-party>
            <as4-party label="To party" [group]="form.get('partyInfo.toParty')"></as4-party>
        
            <div formGroupName="collaborationInfo">
                <h5>Collaboration info</h5>
                <as4-input isLabelBold="true" label="Action" runtimeTooltip="collaborationinfo.action">
                    <input type="text" formControlName="action" />
                </as4-input>
                <as4-input isLabelBold="true" label="Service" runtimeTooltip="collaborationinfo.service"> 
                    <as4-columns formGroupName="service">
                        <input type="text" placeholder="value" formControlName="value" />
                        <input type="text" placeholder="type" formControlName="type" />
                    </as4-columns>
                </as4-input>
                <as4-input isLabelBold="true" label="Agreement reference" formGroupName="agreementReference" runtimeTooltip="collaborationinfo.agreementreference">
                    <as4-columns>
                        <input type="text" formControlName="value" placeholder="value" />
                        <input type="text" formControlName="type" placeholder="type" />
                    </as4-columns>
                </as4-input>
                <as4-input isLabelBold="true" label="PmodeId" formGroupName="agreementReference" runtimeTooltip="collaborationinfo.agreementreference.pmodeid">
                    <as4-pmode-select mode="receiving" formControlName="pModeId"></as4-pmode-select>
                </as4-input>
            </div>
        </div>
    `
})
export class MessagePackagingComponent {
    @Input() form: FormGroup;
}
