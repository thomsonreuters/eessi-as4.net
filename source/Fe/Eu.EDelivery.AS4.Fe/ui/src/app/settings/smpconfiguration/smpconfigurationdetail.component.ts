import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ComponentRef, OnDestroy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable, Subject } from 'rxjs';

import { ItemType } from '../../api/ItemType';
import { SmpConfiguration } from '../../api/SmpConfiguration';
import { RuntimeStore } from '../runtime.store';
import { SmpConfigurationService } from './smpconfiguration.service';

@Component({
  templateUrl: './smpconfigurationdetail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SmpConfigurationDetailComponent implements OnDestroy {
  public form: FormGroup;
  public componentRef: ComponentRef<SmpConfigurationDetailComponent>;
  private componentDestroyed$ = new Subject<any>();
  private defaultValues: ItemType[];
  private isNew: boolean;
  constructor(
    private formBuilder: FormBuilder,
    private service: SmpConfigurationService,
    private runtime: RuntimeStore,
    private changeDetector: ChangeDetectorRef
  ) {
    this.defaultValues = this.runtime.getState().runtimeMetaData;
    this.form = this.buildItem();
  }

  public setRecordId(recordId: number) {
    Observable.of(recordId)
      .flatMap((id) => {
        if (id === 0) {
          this.isNew = true;
          return Observable.of(new SmpConfiguration());
        } else {
          this.isNew = false;
          return this.service.getById(id);
        }
      })
      .subscribe((smp) => {
        this.form.reset(smp, { emitEvent: false });
        this.changeDetector.detectChanges();
      });
  }
  public ngOnDestroy(): void {
    this.componentDestroyed$.next();
  }
  public saveItem(result: boolean) {
    let obs: Observable<boolean>;
    if (result === false) {
      this.componentRef.destroy();
    } else {
      if (this.isNew) {
        obs = this.service
          .create(this.form.value)
          .do(() => this.form.patchValue({ emitEvent: result }))
          .map(() => true);
      } else {
        obs = this.service.update(this.form.value.id, this.form.value);
      }

      obs.subscribe(() => {
        this.componentRef.destroy();
      });
    }
  }
  private buildItem(configuration?: SmpConfiguration): FormGroup {
    const form = this.formBuilder.group({
      [SmpConfiguration.FIELD_id]: [!configuration ? null : configuration.id],
      [SmpConfiguration.FIELD_ToPartyId]: [
        !configuration ? null : configuration.toPartyId,
        Validators.required
      ],
      [SmpConfiguration.FIELD_PartyRole]: [
        !configuration ? null : configuration.partyRole,
        Validators.required
      ],
      [SmpConfiguration.FIELD_PartyType]: [
        !configuration ? null : configuration.partyType,
        Validators.required
      ],
      [SmpConfiguration.FIELD_Url]: [
        !configuration ? null : configuration.url,
        Validators.required
      ],
      [SmpConfiguration.FIELD_ServiceValue]: [
        !configuration ? null : configuration.serviceValue
      ],
      [SmpConfiguration.FIELD_ServiceType]: [
        !configuration ? null : configuration.serviceType
      ],
      [SmpConfiguration.FIELD_Action]: [
        !configuration ? null : configuration.action
      ],
      [SmpConfiguration.FIELD_TlsEnabled]: [
        !configuration ? false : configuration.tlsEnabled,
        Validators.required
      ],
      [SmpConfiguration.FIELD_EncryptionEnabled]: [
        !configuration ? false : configuration.encryptionEnabled,
        Validators.required
      ],
      [SmpConfiguration.FIELD_FinalRecipient]: [
        !configuration ? null : configuration.finalRecipient
      ],
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
          ? this.getDefaultFor(
              SmpConfiguration.FIELD_EncryptKeyTransportAlgorithm
            )
          : configuration.encryptKeyTransportAlgorithm,
        this.isEncryptionEnabled
      ]
    });

    form
      .get(SmpConfiguration.FIELD_EncryptionEnabled)!
      .valueChanges.takeUntil(this.componentDestroyed$)
      .subscribe(() => {
        [
          SmpConfiguration.FIELD_EncryptAlgorithm,
          SmpConfiguration.FIELD_EncryptKeyMgfAlorithm,
          SmpConfiguration.FIELD_EncryptAlgorithmKeySize,
          SmpConfiguration.FIELD_EncryptPublicKeyCertificate,
          SmpConfiguration.FIELD_EncryptKeyDigestAlgorithm,
          SmpConfiguration.FIELD_EncryptKeyTransportAlgorithm
        ].forEach((field) => form.get(field)!.updateValueAndValidity());
      });

    return form;
  }

  private isEncryptionEnabled(x: AbstractControl) {
    if (!x.parent) {
      return null;
    }

    let encryptionEnabled = x.parent!.get(
      SmpConfiguration.FIELD_EncryptionEnabled
    )!.value;
    let isEmpty = !x.value;
    console.log('update');
    return encryptionEnabled && isEmpty
      ? { required: 'This field is required when enabling encryption' }
      : null;
  }

  private getDefaultFor(prop: string) {
    let key = 'smpconfiguration.' + prop.toLowerCase();
    let entry = this.defaultValues[key];
    return entry.defaultvalue;
  }
}
