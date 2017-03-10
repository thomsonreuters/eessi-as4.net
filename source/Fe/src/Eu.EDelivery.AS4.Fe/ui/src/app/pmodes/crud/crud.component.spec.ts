import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule, ReactiveFormsModule, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import {
    inject,
    TestBed,
    ComponentFixture,
    async
} from '@angular/core/testing';
import { Component } from '@angular/core';

import { ICrudPmodeService } from '../crudpmode.service.interface';
import { RuntimeStore } from './../../settings/runtime.store';
import { DialogService } from './../../common/dialog.service';
import { CrudComponent, PMODECRUD_SERVICE } from './crud.component';
import { As4ComponentsModule } from './../../common/as4components.module';
import { IPmode } from './../../api/Pmode.interface';
import { ReceivingPmode } from './../../api/ReceivingPmode';
import { PmodeServiceMock } from './../pmode.service.mock';
import { ReceivingProcessingMode } from './../../api/ReceivingProcessingMode';
import { ModalService } from './../../common/modal/modal.service';

class MockDialogService {
    confirmUnsavedChanges() { }
    prompt(input: string) { }
    message(message: string) { }
    incorrectForm() { }
    deleteConfirm(name: string) { }
}

class RuntimeStoreMock {

}

class ModalServiceMock {
    show(name: string): Observable<boolean> { return null; }
}

describe('Pmode crud', () => {
    let pmodes: Array<string>;
    let instance: CrudComponent;
    let pmode1: IPmode;
    let form: FormGroup;

    beforeEach(async(() => TestBed.configureTestingModule({
        declarations: [

        ],
        imports: [
            ReactiveFormsModule,
            As4ComponentsModule,
            RouterTestingModule
        ],
        providers: [
            CrudComponent,
            { provide: DialogService, useClass: MockDialogService },
            { provide: RuntimeStore, useClass: RuntimeStoreMock },
            { provide: PMODECRUD_SERVICE, useClass: PmodeServiceMock },
            { provide: ModalService, useClass: ModalServiceMock }
        ]
    })));
    beforeEach(inject([CrudComponent], (component: CrudComponent) => {
        instance = component;
    }));
    beforeEach(() => {
        pmodes = new Array<string>();
        pmodes.push('pmode1');
        pmodes.push('pmode2');

        pmode1 = new ReceivingPmode();
        pmode1.name = 'pmode1';
        pmode1.pmode = new ReceivingProcessingMode();
        pmode1.pmode.id = 'pmode1';
        pmode1.pmode.mep = 10;
        form = <FormGroup>{
            dirty: false,
            invalid: false,
            markAsPristine: () => { },
            markAsDirty: () => { }
        };
        instance.form = form;
        instance.pmodes = pmodes;
    });
    describe('when created', () => {
        it('should call service.getForm and subscribe to the store events', inject([PMODECRUD_SERVICE], (service: ICrudPmodeService) => {
            let obsGetAll = spyOn(service, 'obsGetAll').and.returnValue(Observable.of(new Array<string>()));
            let obsGet = spyOn(service, 'obsGet').and.returnValue(Observable.of(null));

            instance.ngOnInit();

            expect(instance.form).toBeDefined();
            expect(obsGetAll).toHaveBeenCalled();
            expect(obsGet).toHaveBeenCalled();
        }));
    });
    describe('when pmode changes', () => {
        it('should result in selecting the pmode when the form is not dirty', inject([PMODECRUD_SERVICE, RuntimeStore], (service: ICrudPmodeService, store: RuntimeStore) => {
            let getPmode = spyOn(service, 'get');
            instance.pmodeChanged('pmode1');

            expect(getPmode).toHaveBeenCalledWith('pmode1');
        }));
        it('should show a dialog when the form is dirty', inject([PMODECRUD_SERVICE, DialogService], (service: ICrudPmodeService, dialogService: DialogService) => {
            let dialogSpy = spyOn(dialogService, 'confirmUnsavedChanges').and.returnValue(Observable.of(true));
            instance.isNewMode = true;
            instance.currentPmode = pmode1;
            instance.pmodeChanged('pmode1');

            expect(dialogSpy).toHaveBeenCalled();
            expect(instance.isNewMode).toBeFalsy();
        }));
    });
    describe('when renaming', () => {
        it('should as the user for a new name', inject([DialogService, PMODECRUD_SERVICE], (dialog: DialogService, service: ICrudPmodeService) => {
            let dialogSpy = spyOn(dialog, 'prompt').and.returnValue(Observable.of(undefined));
            let serviceSpy = spyOn(service, 'patchName');

            instance.rename();

            expect(dialogSpy).toHaveBeenCalled();
            expect(serviceSpy).not.toHaveBeenCalled();
        }));
        it('should call crudService.patchName when confirmed', inject([DialogService, PMODECRUD_SERVICE], (dialog: DialogService, service: ICrudPmodeService) => {
            const renamed = 'RENAMED';
            let dialogSpy = spyOn(dialog, 'prompt').and.returnValue(Observable.of(renamed));
            let serviceSpy = spyOn(service, 'patchName');

            instance.rename();

            expect(dialogSpy).toHaveBeenCalled();
            expect(serviceSpy).toHaveBeenCalledWith(instance.form, renamed);
        }));
        it('should show a message when the pmode name already exists', inject([DialogService, PMODECRUD_SERVICE], (dialog: DialogService, service: ICrudPmodeService) => {
            const renamed = 'pmode1';
            let promptDialogSpy = spyOn(dialog, 'prompt').and.returnValue(Observable.of(renamed));
            let messageDialogSpy = spyOn(dialog, 'message');
            let serviceSpy = spyOn(service, 'patchName');

            instance.rename();

            expect(promptDialogSpy).toHaveBeenCalled();
            expect(messageDialogSpy).toHaveBeenCalled();
            expect(serviceSpy).not.toHaveBeenCalled();
        }));
    });
    describe('when add', () => {
        it('should call modalService', inject([ModalService], (modal: ModalService) => {
            let modalSpy = spyOn(modal, 'show').and.returnValue(Observable.of(false));

            instance.add();

            expect(modalSpy).toHaveBeenCalledWith('new-pmode');
        }));
        it('should not allow a new pmode when the name already exists', inject([ModalService, DialogService, PMODECRUD_SERVICE], (modal: ModalService, dialog: DialogService, service: ICrudPmodeService) => {
            const name = 'pmode1';
            instance.newName = name;
            let modalSpy = spyOn(modal, 'show').and.returnValue(Observable.of(true));
            let dialogSpy = spyOn(dialog, 'message');
            let serviceSpy = spyOn(service, 'getByName');

            instance.add();

            expect(dialogSpy).toHaveBeenCalled();
            expect(serviceSpy).not.toHaveBeenCalled();
        }));
        it('should add the new pmode', inject([ModalService, PMODECRUD_SERVICE], (modal: ModalService, service: ICrudPmodeService) => {
            const name = 'NEWNAME';
            let pMode = new ReceivingPmode();
            instance.newName = name;
            instance.actionType = <any>-1;
            let modalSpy = spyOn(modal, 'show').and.returnValue(Observable.of(true));
            let serviceSpy = spyOn(service, 'getByName').and.returnValue(Observable.of(new ReceivingPmode()));
            let serviceCreateSpy = spyOn(service, 'getNew').and.returnValue(pMode);

            instance.add();

            expect(serviceSpy).not.toHaveBeenCalled();
            expect(serviceCreateSpy).toHaveBeenCalled();
            expect(instance.currentPmode).toBe(pMode);
        }));
        it('should copy existing pmode data when the user has chosen to clone', inject([ModalService, PMODECRUD_SERVICE], (modal: ModalService, service: ICrudPmodeService) => {
            const name = 'NEWNAME';
            let pMode = new ReceivingPmode();
            instance.newName = name;
            instance.actionType = pmode1.name;

            let modalSpy = spyOn(modal, 'show').and.returnValue(Observable.of(true));
            let serviceSpy = spyOn(service, 'getByName').and.returnValue(Observable.of(pmode1));
            let patchSpy = spyOn(service, 'patchForm');

            instance.add();

            expect(serviceSpy).toHaveBeenCalledWith(pmode1.name);
            expect(instance.currentPmode.name).toBe(name);
            expect(instance.currentPmode.pmode.mep).toBe(pmode1.pmode.mep);
            expect(patchSpy).toHaveBeenCalled();
        }));
    });
    describe('when save', () => {
        it('should call dialogService.incorrectForm when form is not valid', inject([DialogService], (dialogService: DialogService) => {
            instance.form.invalid = true;
            let dialogSpy = spyOn(dialogService, 'incorrectForm');

            instance.save();

            expect(dialogSpy).toHaveBeenCalled();
        }));
        it('should call crudService.create when in new mode', inject([PMODECRUD_SERVICE], (service: ICrudPmodeService) => {
            instance.form.invalid = false;
            instance.isNewMode = true;
            let serviceSpy = spyOn(service, 'create').and.returnValue(Observable.of(true));

            instance.save();

            expect(serviceSpy).toHaveBeenCalled();
            expect(instance.isNewMode).toBeFalsy();
        }));
        it('should call crudService.update when not in new mode', inject([PMODECRUD_SERVICE], (service: ICrudPmodeService) => {
            instance.form.invalid = false;
            instance.isNewMode = false;
            instance.currentPmode = pmode1;
            let serviceSpy = spyOn(service, 'update').and.returnValue(Observable.of(true));

            instance.save();

            expect(serviceSpy).toHaveBeenCalled();
            expect(instance.isNewMode).toBeFalsy();
        }));
    });
    describe('when delete', () => {
        it('should do nothing when currentPmode is undefined', inject([DialogService], (dialog: DialogService) => {
            instance.currentPmode = undefined;
            let dialogSpy = spyOn(dialog, 'deleteConfirm');

            instance.delete();

            expect(dialogSpy).not.toHaveBeenCalled();
        }));
        it('should call service.delete and ask for confirmation', inject([DialogService, PMODECRUD_SERVICE], (dialog: DialogService, service: ICrudPmodeService) => {
            instance.currentPmode = pmode1;
            let dialogSpy = spyOn(dialog, 'deleteConfirm').and.returnValue(Observable.of(true));
            let serviceSpy = spyOn(service, 'delete');

            instance.delete();

            expect(dialogSpy).toHaveBeenCalled();
            expect(serviceSpy).toHaveBeenCalledWith(pmode1.name);
        }));
    });
    describe('when reset', () => {
        it('is in newmode then it should be removed from the pmodes collection', inject([PMODECRUD_SERVICE], (service: ICrudPmodeService) => {
            let newPmode = new ReceivingPmode();
            newPmode.name = 'new';
            instance.pmodes = [...instance.pmodes, newPmode.name];
            instance.isNewMode = true;
            instance.currentPmode = newPmode;
            let serviceSpy = spyOn(service, 'patchForm');

            instance.reset();

            expect(instance.isNewMode).toBeFalsy();
            expect(instance.currentPmode).toBeUndefined();
            expect(serviceSpy).toHaveBeenCalledWith(instance.form, instance.currentPmode);
            expect(instance.pmodes.find(pmode => pmode === newPmode.name)).toBeUndefined();
        }));
    });
});
