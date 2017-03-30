import { IPmode } from './../api/Pmode.interface';
import { FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

export interface ICrudPmodeService {
    obsGet(): Observable<IPmode>;
    obsGetAll(): Observable<string[]>;
    get(name: string);
    delete(name: string);
    getNew(name: string): IPmode;
    create(pmode: IPmode): Observable<boolean>;
    getForm(pmode: IPmode): FormGroup;
    getByName(name: string): Observable<IPmode>;
    patchForm(form: FormGroup, pmode: IPmode);
    patchName(form: FormGroup, name: string);
    update(pmode: IPmode, originalName: string): Observable<boolean>;
    getAll();
}