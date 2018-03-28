import 'rxjs/add/operator/debounceTime';

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, OnDestroy } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { Observable } from 'rxjs/Observable';

import { SmpConfiguration } from '../../api/SmpConfiguration';
import { DialogService } from '../../common/dialog.service';
import { SmpConfigurationService } from './smpconfiguration.service';
import { manageError } from '../../helpers';
import { CanComponentDeactivate } from '../../common/candeactivate.guard';
import { RuntimeStore } from '../runtime.store';
import { ItemType } from '../../api/ItemType';
import { Subject } from 'rxjs';

@Component({
    selector: 'as4-smpconfiguration',
    templateUrl: './smpconfiguration.component.html',
    styleUrls: ['./smpconfiguration.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SmpConfigurationComponent implements OnInit, CanComponentDeactivate, OnDestroy {
    public form: FormGroup;
    private defaultValues: ItemType[];
    private componentDestroyed$ = new Subject<any>();
    constructor(
        private formBuilder: FormBuilder,
        private smpConfigurationService: SmpConfigurationService,
        private changeDetector: ChangeDetectorRef,
        private dialogService: DialogService,
        private runtime: RuntimeStore
    ) {
        this.defaultValues = this.runtime.getState().runtimeMetaData;
        this.form = this.formBuilder.group({
            items: this.formBuilder.array([])
        });
        this.reloadConfiguration();
    }
    public ngOnInit(): void {

    }
    public canDeactivate(): boolean {
        return !this.form.dirty;
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
    private isEncryptionEnabled(x: AbstractControl) {
        if (!x.parent) {
            return null;
        }

        let encryptionEnabled = x.parent!.get(SmpConfiguration.FIELD_EncryptionEnabled)!.value;
        let isEmpty = !x.value;
        console.log("update");
        return encryptionEnabled && isEmpty ? { required: "This field is required when enabling encryption" } : null;
    }
    private buildItem(configuration?: SmpConfiguration): FormGroup {
        const form = this.formBuilder.group({
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
            [SmpConfiguration.FIELD_EncryptAlgorithm]: [
                !configuration ? null : configuration.encryptAlgorithm,
                this.isEncryptionEnabled
            ],
            [SmpConfiguration.FIELD_EncryptAlgorithmKeySize]: [
                !configuration || configuration.encryptAlgorithmKeySize
                    ? this.getDefaultFor(SmpConfiguration.FIELD_EncryptAlgorithmKeySize)
                    : configuration.encryptAlgorithmKeySize,
                this.isEncryptionEnabled
            ],
            [SmpConfiguration.FIELD_EncryptPublicKeyCertificate]: [
                !configuration ? null : configuration.encryptPublicKeyCertificate,
                this.isEncryptionEnabled
            ],
            [SmpConfiguration.FIELD_EncryptPublicKeyCertificateName]: [
                !configuration ? null : configuration.encryptPublicKeyCertificateName
            ],
            [SmpConfiguration.FIELD_EncryptKeyDigestAlgorithm]: [
                !configuration
                    ? this.getDefaultFor(SmpConfiguration.FIELD_EncryptKeyDigestAlgorithm)
                    : configuration.encryptKeyDigestAlgorithm,
                this.isEncryptionEnabled
            ],
            [SmpConfiguration.FIELD_EncryptKeyMgfAlorithm]: [
                !configuration ? null : configuration.encryptKeyMgfAlorithm,
                this.isEncryptionEnabled
            ],
            [SmpConfiguration.FIELD_EncryptKeyTransportAlgorithm]: [
                !configuration
                    ? this.getDefaultFor(SmpConfiguration.FIELD_EncryptKeyTransportAlgorithm)
                    : configuration.encryptKeyTransportAlgorithm,
                this.isEncryptionEnabled
            ]
        }
        );

        form.get(SmpConfiguration.FIELD_EncryptionEnabled)!
            .valueChanges
            .takeUntil(this.componentDestroyed$)
            .subscribe(() => {
                [
                    SmpConfiguration.FIELD_EncryptAlgorithm,
                    SmpConfiguration.FIELD_EncryptKeyMgfAlorithm,
                    SmpConfiguration.FIELD_EncryptAlgorithmKeySize,
                    SmpConfiguration.FIELD_EncryptPublicKeyCertificate,
                    SmpConfiguration.FIELD_EncryptKeyDigestAlgorithm,
                    SmpConfiguration.FIELD_EncryptKeyTransportAlgorithm
                ].forEach(field => form.get(field)!.updateValueAndValidity());
            });

        return form;
    }
    public ngOnDestroy(): void {
        this.componentDestroyed$.next();
    }
    private getDefaultFor(prop: string) {
        let key = "smpconfiguration." + prop.toLowerCase();
        let entry = this.defaultValues[key];
        return entry.defaultvalue;
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
