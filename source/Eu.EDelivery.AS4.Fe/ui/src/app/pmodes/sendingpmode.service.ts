import { Injectable } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';

import { IPmode } from './../api/Pmode.interface';
import { SendingPmode } from './../api/SendingPmode';
import { SendingPmodeForm } from './../api/SendingPmodeForm';
import { SendingProcessingMode } from './../api/SendingProcessingMode';
import { FormBuilderExtended, FormWrapper } from './../common/form.service';
import { RuntimeStore } from './../settings/runtime.store';
import { ICrudPmodeService } from './crudpmode.service.interface';
import { PmodeStore } from './pmode.store';

@Injectable()
export class SendingPmodeService implements ICrudPmodeService {
  private _baseUrl = 'api/pmode/sending';
  private _form: FormWrapper;
  constructor(
    private _http: AuthHttp,
    private _pmodeStore: PmodeStore,
    private _formBuilder: FormBuilderExtended,
    private _runtimeStore: RuntimeStore
  ) {}
  public getAll() {
    this._http.get(this.getBaseUrl()).subscribe((result) => {
      this._pmodeStore.update('SendingNames', result.json());
    });
  }
  public obsGet(): Observable<IPmode | undefined> {
    return this._pmodeStore.changes
      .filter((result) => !!result)
      .map((result) => result.Sending)
      .distinctUntilChanged();
  }
  public obsGetAll(): Observable<string[] | undefined> {
    return this._pmodeStore.changes
      .filter((result) => !!result)
      .map((result) => result.SendingNames)
      .distinctUntilChanged();
  }
  public get(name: string) {
    if (!!!name) {
      this._pmodeStore.update('Sending', null);
      return;
    }

    this._http
      .get(`${this.getBaseUrl()}/${name}`)
      .subscribe((result) => this._pmodeStore.update('Sending', result.json()));
  }
  public delete(name: string, onlyStore: boolean = false) {
    if (onlyStore) {
      this._pmodeStore.deleteSending(name);
      return;
    }
    this._http.delete(`${this.getBaseUrl()}/${name}`).subscribe((result) => {
      this._pmodeStore.deleteSending(name);
    });
  }
  public update(pmode: IPmode, originalName: string): Observable<boolean> {
    if ((<SendingPmode> pmode).isDynamicDiscoveryEnabled) {
      (<SendingProcessingMode> pmode.pmode).pushConfiguration = undefined;
    } else {
      (<SendingProcessingMode> pmode.pmode).dynamicDiscovery = undefined;
    }

    let obs = new Subject<boolean>();
    this._http
      .put(`${this.getBaseUrl()}/${originalName}`, pmode)
      .subscribe((result) => {
        this._pmodeStore.setSending(<SendingPmode> pmode);
        obs.next(true);
        obs.complete();
      });
    return obs.asObservable();
  }
  public getNew(name: string): IPmode {
    let newPmode = new SendingPmode();
    newPmode.name = name;
    newPmode.pmode = new SendingProcessingMode();
    newPmode.pmode.id = name;
    newPmode.type = 0;
    return newPmode;
  }
  public create(pmode: IPmode): Observable<boolean> {
    let obs = new Subject<boolean>();
    this._http.post(`${this.getBaseUrl()}`, pmode).subscribe((result) => {
      this._pmodeStore.setSending(<SendingPmode> pmode);
      obs.next();
      obs.complete();
    });
    return obs.asObservable();
  }
  public getForm(pmode: IPmode): FormWrapper {
    if (!!!this._form) {
      this._form = this._formBuilder.get();
    }
    return SendingPmodeForm.getForm(
      this._form,
      <SendingPmode> pmode,
      this._runtimeStore.state.runtimeMetaData
    );
  }
  public patchName(form: FormGroup, name: string) {
    form.patchValue({
      [SendingPmode.FIELD_name]: name,
      [SendingPmode.FIELD_pmode]: {
        [SendingProcessingMode.FIELD_id]: name
      }
    });
  }
  public getByName(name: string): Observable<IPmode> {
    let obs = new Subject<SendingPmode>();
    this._http.get(`${this.getBaseUrl()}/${name}`).subscribe((result) => {
      obs.next(result.json());
      obs.complete();
    });
    return obs.asObservable();
  }
  private getBaseUrl(action?: string): string {
    if (!!!action) {
      return this._baseUrl;
    }
    return `${this._baseUrl}/action`;
  }
}
