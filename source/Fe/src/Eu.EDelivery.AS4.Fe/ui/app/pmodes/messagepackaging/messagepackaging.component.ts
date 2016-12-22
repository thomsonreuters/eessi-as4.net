import { FormGroup } from '@angular/forms';
import { Component, Input } from '@angular/core';

@Component({
    selector: 'as4-message-packaging',
    template: `
        <div [formGroup]="form">
            <as4-party label="From party" [group]="form.controls.partyInfo.controls.fromParty"></as4-party>
            <as4-party label="To party" [group]="form.controls.partyInfo.controls.toParty"></as4-party>
        
            <div formGroupName="collaborationInfo">
                <h5>Collaboration info</h5>
                <as4-input isLabelBold="true" labelSize="2" controlSize="4" label="Action">
                    <input type="text" formControlName="action" />
                </as4-input>
                <as4-input isLabelBold="true" labelSize="2" controlSize="4" label="Service">
                    <as4-columns formGroupName="service">
                        <input type="text" placeholder="value" formControlName="value" />
                        <input type="text" placeholder="type" formControlName="type" />
                    </as4-columns>
                </as4-input>
                <as4-input isLabelBold="true" label="Agreement reference" labelSize="2" controlSize="4" formGroupName="agreementReference">
                    <as4-columns>
                        <input type="text" formControlName="value" placeholder="value" />
                        <input type="text" formControlName="type" placeholder="type" />
                    </as4-columns>
                </as4-input>
                <as4-input labelSize="2" isLabelBold="true" controlSize="4" label="PmodeId" formGroupName="agreementReference">
                    <as4-pmode-select mode="receiving" formControlName="pModeId"></as4-pmode-select>
                </as4-input>
            </div>
        </div>
    `
})
export class MessagePackagingComponent {
    @Input() form: FormGroup;
}
