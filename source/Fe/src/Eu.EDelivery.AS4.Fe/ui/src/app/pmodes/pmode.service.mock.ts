import { FormGroup } from '@angular/forms';
import { IPmode } from './../api/Pmode.interface';
import { ICrudPmodeService } from './pmode.service';
import { Observable } from 'rxjs';

export class PmodeServiceMock implements ICrudPmodeService {
    obsGet(): Observable<IPmode> { return null; }
    obsGetAll(): Observable<Array<string>> { return null; }
    get(name: string) { }
    delete(name: string) { }
    getNew(name: string): IPmode { return null; }
    create(pmode: IPmode): Observable<boolean> { return null; }
    getForm(pmode: IPmode): FormGroup { return null; }
    getByName(name: string): Observable<IPmode> { return null; }
    patchForm(form: FormGroup, pmode: IPmode) { }
    patchName(form: FormGroup, name: string) { }
    update(pmode: IPmode, originalName: string): Observable<boolean> { return null; }
    getAll() { }
}
