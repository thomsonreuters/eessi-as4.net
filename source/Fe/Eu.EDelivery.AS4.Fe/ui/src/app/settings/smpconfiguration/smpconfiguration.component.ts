import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';

import { SmpConfiguration } from '../../api/SmpConfiguration';

@Component({
    selector: 'as4-smpconfiguration',
    templateUrl: './smpconfiguration.component.html',
    styleUrls: ['./smpconfiguration.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SmpConfigurationComponent {
    public form: FormGroup;
    constructor(private formBuilder: FormBuilder) {
        this.form = this.formBuilder.group({
            items: this.formBuilder.array([])
        });
    }
    public get itemsControl(): FormArray {
        return <FormArray>this.form.get('items');
    }
    public addItem() {
        this.itemsControl.push(this.buildItem());
    }
    public removeItem(index: number) {
        this.itemsControl.removeAt(index);
    }
    private buildItem(configuration?: SmpConfiguration): FormGroup{
        return this.formBuilder.group({
            id: [!configuration ? null : configuration.id],
            toPartyId: [!configuration ? null : configuration.toPartyId],
            partyRole: [!configuration ? null : configuration.partyRole],
            partyType: [!configuration ? null : configuration.partyType],
            url: [!configuration ? null : configuration.url],
            serviceValue: [!configuration ? null : configuration.serviceValue],
            serviceType: [!configuration ? null : configuration.serviceType],
            action: [!configuration ? null : configuration.action],
            tlsEnabled: [!configuration ? null : configuration.tlsEnabled],
            encryptionEnabled: [!configuration ? null : configuration.encryptionEnabled],
            finalRecipient: [!configuration ? null : configuration.finalRecipient],
            encryptAlgorithm: [!configuration ? null : configuration.encryptAlgorithm],
            encryptAlgorithmKeySize: [!configuration ? null : configuration.encryptAlgorithmKeySize],
            encryptPublicKeyCertificate: [!configuration ? null : configuration.encryptPublicKeyCertificate],
            encryptKeyDigestAlgorithm: [!configuration ? null : configuration.encryptKeyDigestAlgorithm],
            encryptKeyMgfAlorithm: [!configuration ? null : configuration.encryptKeyMgfAlorithm],
            encryptKeyTransportAlgorithm: [!configuration ? null : configuration.encryptKeyTransportAlgorithm]
        });
    }
}
