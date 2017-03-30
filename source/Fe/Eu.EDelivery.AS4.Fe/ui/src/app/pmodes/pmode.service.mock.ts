import { FormGroup } from '@angular/forms';
import { IPmode } from './../api/Pmode.interface';
import { ICrudPmodeService } from './crudpmode.service.interface';
import { Observable } from 'rxjs';

export class PmodeServiceMock implements ICrudPmodeService {
   public obsGet(): Observable<IPmode> { return null; }
   public obsGetAll(): Observable<string[]> { return null; }
   public get(name: string) { }
   public delete(name: string) { }
   public getNew(name: string): IPmode { return null; }
   public create(pmode: IPmode): Observable<boolean> { return null; }
   public getForm(pmode: IPmode): FormGroup { return null; }
   public getByName(name: string): Observable<IPmode> { return null; }
   public patchForm(form: FormGroup, pmode: IPmode) { }
   public patchName(form: FormGroup, name: string) { }
   public update(pmode: IPmode, originalName: string): Observable<boolean> { return null; }
   public getAll() { }
}
