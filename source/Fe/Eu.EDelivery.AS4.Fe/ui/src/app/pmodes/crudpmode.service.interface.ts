import { FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import { FormWrapper } from './../common/form.service';
import { IPmode } from './../api/Pmode.interface';

export interface ICrudPmodeService {
    obsGet(): Observable<IPmode>;
    obsGetAll(): Observable<string[]>;
    get(name: string);
    delete(name: string);
    getNew(name: string): IPmode;
    create(pmode: IPmode): Observable<boolean>;
    getForm(pmode: IPmode): FormWrapper;
    getByName(name: string): Observable<IPmode>;
    patchName(form: FormGroup, name: string);
    update(pmode: IPmode, originalName: string): Observable<boolean>;
    getAll();
}
