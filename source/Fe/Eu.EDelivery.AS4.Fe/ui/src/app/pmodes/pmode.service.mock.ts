import { FormGroup } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { IPmode } from './../api/Pmode.interface';
import { ICrudPmodeService } from './crudpmode.service.interface';
import { Observable } from 'rxjs';

export class PmodeServiceMock implements ICrudPmodeService {
   public obsGet(): Observable<IPmode | undefined> { return Observable.of(undefined); }
   public obsGetAll(): Observable<string[] | undefined> {  return Observable.of(undefined); }
   public get(name: string) { }
   public delete(name: string) { }
   public getNew(name: string): IPmode { return undefined!; }
   public create(pmode: IPmode): Observable<boolean> { return undefined!; }
   public getForm(pmode: IPmode): FormWrapper { return undefined!; }
   public getByName(name: string): Observable<IPmode> { return undefined!; }
   public patchName(form: FormGroup, name: string) { }
   public update(pmode: IPmode, originalName: string): Observable<boolean> { return undefined!; }
   public getAll() { }
}
