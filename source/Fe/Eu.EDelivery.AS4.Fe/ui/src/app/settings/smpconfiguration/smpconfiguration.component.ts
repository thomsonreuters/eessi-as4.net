import 'rxjs/add/operator/debounceTime';

import { ChangeDetectionStrategy, ChangeDetectorRef, Component } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { Observable } from 'rxjs/Observable';

import { SmpConfiguration } from '../../api/SmpConfiguration';
import { DialogService } from '../../common/dialog.service';
import { SmpConfigurationService } from './smpconfiguration.service';
import { manageError } from '../../helpers';

@Component({
    selector: 'as4-smpconfiguration',
    templateUrl: './smpconfiguration.component.html',
    styleUrls: ['./smpconfiguration.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SmpConfigurationComponent {
    public form: FormGroup;
    constructor(
        private formBuilder: FormBuilder,
        private smpConfigurationService: SmpConfigurationService,
        private changeDetector: ChangeDetectorRef,
        private dialogService: DialogService
    ) {
        this.form = this.formBuilder.group({
            items: this.formBuilder.array([])
        });
        this.reloadConfiguration();
    }
    public get itemsControl(): FormArray {
        return <FormArray>this.form.get('items');
    }
    public addItem() {
        this.itemsControl.push(this.buildItem());
    }
    public saveItem(configuration: FormGroup) {
        let obs: Observable<boolean>;
        if (!configuration.value.id) {
            obs = this.smpConfigurationService
                .create(configuration.value)
                .do((result) => {
                    configuration.patchValue(result);
                })
                .map(() => true);
        } else {
            obs = this.smpConfigurationService.update(configuration.value.id, configuration.value);
        }

        obs.subscribe(() => {
            configuration.markAsUntouched();
            configuration.markAsPristine();
        });
    }
    public deleteItem(configuration: SmpConfiguration, index: number) {
        this.dialogService.confirm('Are you sure you want to delete the configuration?').subscribe((result) => {
            if (!!result) {
                this.itemsControl.removeAt(index);
                if (!!configuration.id) {
                    this.smpConfigurationService.delete(configuration.id).subscribe();
                }
                this.changeDetector.markForCheck();
            }
        });
    }
    private buildItem(configuration?: SmpConfiguration): FormGroup {
        return this.formBuilder.group({
            [SmpConfiguration.FIELD_id]: [!configuration ? null : configuration.id],
            [SmpConfiguration.FIELD_ToPartyId]: [!configuration ? null : configuration.toPartyId, Validators.required],
            [SmpConfiguration.FIELD_PartyRole]: [!configuration ? null : configuration.partyRole, Validators.required],
            [SmpConfiguration.FIELD_PartyType]: [!configuration ? null : configuration.partyType, Validators.required],
            [SmpConfiguration.FIELD_Url]: [!configuration ? null : configuration.url, Validators.required],
            [SmpConfiguration.FIELD_ServiceValue]: [!configuration ? null : configuration.serviceValue],
            [SmpConfiguration.FIELD_ServiceType]: [!configuration ? null : configuration.serviceType],
            [SmpConfiguration.FIELD_Action]: [!configuration ? null : configuration.action],
            [SmpConfiguration.FIELD_TlsEnabled]: [
                !configuration ? false : configuration.tlsEnabled,
                Validators.required
            ],
            [SmpConfiguration.FIELD_EncryptionEnabled]: [
                !configuration ? false : configuration.encryptionEnabled,
                Validators.required
            ],
            [SmpConfiguration.FIELD_FinalRecipient]: [!configuration ? null : configuration.finalRecipient],
            [SmpConfiguration.FIELD_EncryptAlgorithm]: [!configuration ? null : configuration.encryptAlgorithm],
            [SmpConfiguration.FIELD_EncryptAlgorithmKeySize]: [
                !configuration || configuration.encryptAlgorithmKeySize ? 0 : configuration.encryptAlgorithmKeySize,
                Validators.required
            ],
            [SmpConfiguration.FIELD_EncryptPublicKeyCertificate]: [
                !configuration ? null : configuration.encryptPublicKeyCertificate
            ],
            [SmpConfiguration.FIELD_EncryptPublicKeyCertificateName]: [
                !configuration ? null : configuration.encryptPublicKeyCertificateName
            ],
            [SmpConfiguration.FIELD_EncryptKeyDigestAlgorithm]: [
                !configuration ? null : configuration.encryptKeyDigestAlgorithm
            ],
            [SmpConfiguration.FIELD_EncryptKeyMgfAlorithm]: [
                !configuration ? null : configuration.encryptKeyMgfAlorithm
            ],
            [SmpConfiguration.FIELD_EncryptKeyTransportAlgorithm]: [
                !configuration ? null : configuration.encryptKeyTransportAlgorithm
            ]
        });
    }
    private reloadConfiguration() {
        this.smpConfigurationService
            .get()
            .map((items) => items.map((item) => this.buildItem(item)))
            .subscribe((formArray) => {
                this.form = this.formBuilder.group({
                    items: this.formBuilder.array(formArray, this.validateUniqueEntries)
                });
                this.changeDetector.markForCheck();
            });
    }
    private validateUniqueEntries = (formArray: FormArray) => {
        for (let group of formArray.controls) {
            this.setError(group, this.exists(group, formArray));
        }
        return null;
        // tslint:disable-next-line:semicolon
    };
    private exists(formGroup: AbstractControl, formArray: FormArray): boolean {
        for (let group of formArray.controls.filter((searchGroup) => searchGroup !== formGroup)) {
            if (
                group.get(SmpConfiguration.FIELD_ToPartyId)!.value ===
                    formGroup.get(SmpConfiguration.FIELD_ToPartyId)!.value &&
                group.get(SmpConfiguration.FIELD_PartyRole)!.value ===
                    formGroup.get(SmpConfiguration.FIELD_PartyRole)!.value &&
                group.get(SmpConfiguration.FIELD_PartyType)!.value ===
                    formGroup.get(SmpConfiguration.FIELD_PartyType)!.value
            ) {
                return true;
            }
        }
        return false;
    }
    private setError(formGroup: AbstractControl, set: boolean = false) {
        manageError(formGroup.get(SmpConfiguration.FIELD_ToPartyId)!, 'alreadyExists', set);
        manageError(formGroup.get(SmpConfiguration.FIELD_PartyRole)!, 'alreadyExists', set);
        manageError(formGroup.get(SmpConfiguration.FIELD_PartyType)!, 'alreadyExists', set);
    }
}
