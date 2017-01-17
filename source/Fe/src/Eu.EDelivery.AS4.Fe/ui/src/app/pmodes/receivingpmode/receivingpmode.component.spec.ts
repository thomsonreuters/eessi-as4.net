// import { PmodesModule } from './../pmodes.module';
// import { As4ComponentsModule } from './../../common/as4components.module';
// import { WrapperComponent } from './../../common/wrapper.component';
// import { ActivatedRoute, RouterModule } from '@angular/router';
// import { Decryption } from './../../api/Decryption';
// import { ReceiveSecurity } from './../../api/ReceiveSecurity';
// import { ModalService } from '../../common/modal/modal.service';
// import { RuntimeStore } from './../../settings/runtime.store';
// import { ReceivingProcessingMode } from './../../api/ReceivingProcessingMode';
// import { FormBuilder } from '@angular/forms';
// import { Component } from '@angular/core';
// import {
//     BaseRequestOptions,
//     ConnectionBackend,
//     Http
// } from '@angular/http';
// import { MockBackend } from '@angular/http/testing';
// import { RouterTestingModule } from '@angular/router/testing';

// import { ReceivingPmodeComponent } from './receivingpmode.component';
// import { PmodeService, pmodeService } from '../pmode.service';
// import { Observable } from 'rxjs';
// import {
//     async,
//     fakeAsync,
//     inject,
//     TestBed
// } from '@angular/core/testing';

// import { PmodeStore } from '../pmode.store';
// import { PmodeServiceMock } from '../pmode.service.mock';
// import { DialogService } from './../../common/dialog.service';
// import { ReceivingPmode } from './../../api/ReceivingPmode';
// import { ROUTES } from './../pmodes.routes';

// describe('receiving pmode', () => {
//     let pmodes: Array<string>;
//     let pmode1: ReceivingPmode;
//     let pmode2: ReceivingPmode;
//     beforeEach(() => TestBed.configureTestingModule({
//         imports: [
//             RouterTestingModule.withRoutes(ROUTES),
//             As4ComponentsModule,
//             PmodesModule
//         ],
//         providers: [
//             ReceivingPmodeComponent,
//             { provide: PmodeService, useClass: PmodeServiceMock },
//             FormBuilder,
//             PmodeStore,
//             DialogService,
//             ModalService,
//             RuntimeStore
//         ]
//     }));
//     beforeEach(() => {
//         pmodes = new Array<string>();
//         pmodes.push('pmode1');
//         pmodes.push('pmode2');

//         pmode1 = new ReceivingPmode();
//         pmode1.name = 'pmode1';
//         pmode1.pmode = new ReceivingProcessingMode();
//         pmode1.pmode.id = 'pmode1';

//         pmode2 = new ReceivingPmode();
//         pmode2.name = 'pmode2';
//         pmode2.pmode = new ReceivingProcessingMode();
//         pmode2.pmode.id = 'pmode2';
//     });
//     let cmp: ReceivingPmodeComponent;
//     let modalService: ModalService;
//     let dialogService: DialogService;
//     let store: PmodeStore;
//     beforeEach(inject([ReceivingPmodeComponent, ModalService, DialogService, PmodeStore], (_cmp: ReceivingPmodeComponent, _modalService: ModalService, _dialogService: DialogService, _pmodeStore: PmodeStore) => {
//         cmp = _cmp;

//         modalService = _modalService;

//         dialogService = _dialogService;

//         store = _pmodeStore;
//         store.setReceivingNames(pmodes);

//         cmp.actionType = -1;
//         cmp.init();
//     }));
//     describe('current pmode change', () => {
//         it('should call setReceiving when name is defined', inject([PmodeService], (pmodeService: PmodeService) => {
//             let dialogSpy = spyOn(dialogService, 'confirmUnsavedChanges').and.returnValue(Observable.of(true));
//             let serviceSpy = spyOn(pmodeService, 'setReceiving');

//             cmp.pmodeChanged('pmode1');

//             expect(serviceSpy).toHaveBeenCalledWith('pmode1');
//         }));
//         it('should set the currentPmode to undefined when selected name is "select an option" and form should be reset', inject([], () => {
//             let dialogSpy = spyOn(dialogService, 'confirmUnsavedChanges').and.returnValue(Observable.of(true));
//             store.setReceiving(pmode1);
//             let storeSpy = spyOn(store, 'setReceiving');
//             cmp.pmodeChanged('Select an option');

//             expect(storeSpy).toHaveBeenCalledWith(undefined);
//         }));
//         it('should ask the user for confirmation when the form is dirty', inject([], () => {
//             let dialogSpy = spyOn(dialogService, 'confirmUnsavedChanges').and.returnValue(Observable.of(true));
//             cmp.isNewMode = true;
//             store.setReceiving(pmode1);
//             cmp.form.markAsDirty();

//             cmp.pmodeChanged(pmode2.name);

//             expect(dialogSpy).toHaveBeenCalled();
//             expect(cmp.isNewMode).toBeFalsy();
//         }));
//         it('should ask the user for confirmation when the component is in new mode', inject([], () => {
//             let dialogSpy = spyOn(dialogService, 'confirmUnsavedChanges').and.returnValue(Observable.of(false));
//             spyOn(modalService, 'show').and.returnValue(Observable.of(true));
//             cmp.newName = 'new';

//             cmp.add();
//             cmp.form.markAsPristine();

//             cmp.pmodeChanged(pmode1.name);

//             expect(dialogSpy).toHaveBeenCalled();
//             expect(cmp.currentPmode.name).toBe('new');
//         }));
//     });
//     describe('store events', () => {
//         it('should update currentPmode on event', inject([], () => {
//             store.setReceiving(pmode1);
//             expect(cmp.currentPmode).toBe(pmode1);

//             store.setReceiving(undefined);
//             expect(cmp.currentPmode).toBeUndefined();
//         }));
//         it('should update pmodes on event', inject([], () => {
//             store.setReceivingNames(pmodes);
//             expect(cmp.pmodes).toBe(pmodes);

//             store.setReceivingNames(undefined);
//             expect(cmp.pmodes).toBeUndefined();
//         }));
//         it('should set currentPmode to null on delete', inject([], () => {
//             store.setReceivingNames(pmodes);
//             store.setReceiving(pmode1);

//             expect(cmp.currentPmode).toBe(pmode1);

//             store.deleteReceiving(pmode1.name);

//             expect(cmp.currentPmode).toBeNull();
//         }));
//     });
//     describe('form', () => {
//         it('should be disabled when currentPmode is undefined', inject([], () => {
//             async(() => {
//                 let formSpy = spyOn(cmp.form, 'disable');

//                 cmp.currentPmode = undefined;

//                 expect(formSpy).toHaveBeenCalled();
//             });
//         }));
//         it('should be enabled when currentPmode is defined', inject([], () => {
//             async(() => {
//                 let formSpy = spyOn(cmp.form, 'enable');

//                 cmp.currentPmode = pmode1;

//                 expect(formSpy).toHaveBeenCalled();
//             });
//         }));
//     });
//     describe('rename', () => {
//         it('should ask the user for a name and set the new name and mark the form as dirty', inject([], () => {
//             store.setReceiving(pmode1);
//             expect((<ReceivingPmode>cmp.form.value).name).toBe(pmode1.name);
//             let dialogSpy = spyOn(dialogService, 'prompt').and.returnValue(Observable.of('renamed'));
//             let formSpy = spyOn(cmp.form, 'markAsDirty');

//             cmp.rename();

//             expect(dialogSpy).toHaveBeenCalled();
//             expect((<ReceivingPmode>cmp.form.value).name).toBe('renamed');
//             expect(formSpy).toHaveBeenCalled();
//         }));
//         it('should do nothing when the user cancels', inject([], () => {
//             store.setReceiving(pmode1);
//             expect((<ReceivingPmode>cmp.form.value).name).toBe(pmode1.name);
//             let dialogSpy = spyOn(dialogService, 'prompt').and.returnValue(Observable.of(undefined));

//             cmp.rename();

//             expect((<ReceivingPmode>cmp.form.value).name).toBe(pmode1.name);
//         }));
//         it('should not allow a pmode to be renamed to an existing one', inject([], () => {
//             store.setReceiving(pmode1);
//             spyOn(dialogService, 'prompt').and.returnValue(Observable.of(pmode2.name.toUpperCase()));
//             let alreadyExistsDialog = spyOn(dialogService, 'message');
//             cmp.rename();

//             expect(cmp.form.get('name').value).toBe(pmode1.name);
//             expect(alreadyExistsDialog).toHaveBeenCalledWith(`Pmode with name ${pmode2.name.toUpperCase()} already exists`);
//         }));
//     });
//     describe('reset', () => {
//         it('should revert to the currentPmode value and mark the form as pristine', inject([], () => {
//             store.setReceiving(pmode1);
//             let dialogServiceSpy = spyOn(dialogService, 'prompt').and.returnValue(Observable.of('renamed'));
//             let formSpy = spyOn(cmp.form, 'markAsPristine');

//             cmp.rename();
//             expect((<ReceivingPmode>cmp.form.value).name).toBe('renamed');

//             cmp.reset();

//             expect(cmp.currentPmode.name).toBe(pmode1.name);
//             expect((<ReceivingPmode>cmp.form.value).name).toBe(pmode1.name);
//             expect(formSpy).toHaveBeenCalled();
//             expect(cmp.pmodes.filter(pmode => pmode === pmode1.name));
//         }));
//         it('should cleanup from newMode', inject([], () => {
//             spyOn(modalService, 'show').and.returnValue(Observable.of(true));
//             cmp.newName = 'newPmode';
//             cmp.add();
//             expect(cmp.currentPmode.name).toBe('newPmode');

//             cmp.reset();

//             expect(cmp.isNewMode).toBeFalsy();
//             expect(cmp.pmodes.find(pmode => pmode === 'newPmode')).toBeUndefined();
//             expect(cmp.currentPmode).toBeUndefined();
//             expect((<ReceivingPmode>cmp.form.value).name).toBeUndefined();
//         }));
//     });
//     describe('delete', () => {
//         it('should call pmodeservice', inject([PmodeService], (service: PmodeService) => {
//             spyOn(dialogService, 'deleteConfirm').and.returnValue(Observable.of(true));
//             store.setReceiving(pmode1);
//             let serviceSpy = spyOn(service, 'deleteReceiving');

//             cmp.delete();

//             expect(serviceSpy).toHaveBeenCalled();
//         }));
//         it('should not trigger service when currentPmode is undefined', inject([PmodeService], (service: PmodeService) => {
//             spyOn(dialogService, 'deleteConfirm').and.returnValue(Observable.of(true));
//             let serviceSpy = spyOn(service, 'deleteReceiving');

//             cmp.delete();

//             expect(serviceSpy).not.toHaveBeenCalled();
//         }));
//         it('should ask the user for confirmation and call pmodeservice when the user confirmed', inject([PmodeService], (service: PmodeService) => {
//             cmp.currentPmode = pmode1;
//             let dialogSpy = spyOn(dialogService, 'deleteConfirm').and.returnValue(Observable.of(true));
//             let serviceSpy = spyOn(service, 'deleteReceiving');

//             cmp.delete();

//             expect(dialogSpy).toHaveBeenCalled();
//             expect(serviceSpy).toHaveBeenCalled();
//         }));
//         it('should ask the user for confirmation and dont do anything when the user cancelled', inject([PmodeService], (service: PmodeService) => {
//             cmp.currentPmode = pmode1;
//             let dialogSpy = spyOn(dialogService, 'deleteConfirm').and.returnValue(Observable.of(false));
//             let serviceSpy = spyOn(service, 'deleteReceiving');

//             cmp.delete();

//             expect(dialogSpy).toHaveBeenCalled();
//             expect(serviceSpy).not.toHaveBeenCalled();
//         }));
//     });
//     describe('add', () => {
//         it('should ask the user for a name and set currentPmode and isNew', inject([], () => {
//             spyOn(modalService, 'show').and.returnValue(Observable.of(true));
//             cmp.newName = 'new';

//             cmp.add();

//             expect(cmp.isNewMode).toBeTruthy();
//             expect(cmp.currentPmode.name).toBe('new');
//             expect(cmp.currentPmode.pmode.id).toBe('new');
//             expect((<ReceivingPmode>cmp.form.value).name).toBe('new');
//             expect(cmp.form.dirty).toBeTruthy();
//         }));
//         it('should not allow a pmode with an already existing name', inject([], () => {
//             spyOn(modalService, 'show').and.returnValue(Observable.of(true));
//             cmp.newName = pmode1.name.toUpperCase();
//             let existsSpy = spyOn(dialogService, 'message');

//             cmp.add();

//             expect(cmp.isNewMode).toBeFalsy();
//             expect(existsSpy).toHaveBeenCalledWith(`Pmode with name ${pmode1.name.toUpperCase()} already exists`);
//         }));
//         describe('modal new-pmode dialog', () => {
//             let newName: string = 'newName';
//             beforeEach(inject([], () => {
//                 spyOn(modalService, 'show').and.returnValue(Observable.of(true));
//                 cmp.newName = newName;
//                 cmp.actionType = -1;
//             }));
//             it('add must show the new-pmode modal', () => {
//                 cmp.add();

//                 expect(modalService.show).toHaveBeenCalledWith('new-pmode');
//             });
//             it('add must use the actiontype', inject([PmodeService], (pmodeService: PmodeService) => {
//                 cmp.actionType = 0;
//                 spyOn(pmodeService, 'getReceivingByName').and.returnValue(Observable.of(pmode1));

//                 cmp.add();

//                 expect(cmp.currentPmode.name).toBe(newName);
//                 expect(cmp.currentPmode.pmode.id).toBe(pmode1.pmode.id);
//             }));
//             it('should be possible to save a new custom pmode', async(() => {
//                 cmp.actionType = -1;
//                 cmp.newName = 'test';

//                 cmp.add();

//                 setTimeout(() => {
//                     expect(cmp.form.get('name').value).toBe('test');
//                 });
//             }));
//         });
//     });
//     describe('save', () => {
//         it('should call pmodeservice.createreceiving when in newmode', inject([PmodeService], (pmodeService: PmodeService) => {
//             let newName = 'newName';
//             spyOn(modalService, 'show').and.returnValue(Observable.of(true));
//             cmp.newName = newName;

//             let serviceSpy = spyOn(pmodeService, 'createReceiving').and.returnValue(Observable.of(true));
//             cmp.add();
//             let formSpy = {
//                 invalid: false,
//                 markAsPristine: () => { },
//                 disable: () => { },
//                 enable: () => { }
//             };
//             cmp.form = <any>formSpy;

//             cmp.save();

//             expect(serviceSpy).toHaveBeenCalled();
//             expect(cmp.form.dirty).toBeFalsy();
//         }));
//         it('should call pmodeservice.updatereceiving when not in newmode', inject([PmodeService], (pmodeService: PmodeService) => {
//             store.setReceiving(pmode1);
//             let serviceSpy = spyOn(pmodeService, 'updateReceiving').and.returnValue(Observable.of(true));
//             let formSpy = {
//                 invalid: false,
//                 markAsPristine: () => { },
//                 disable: () => { },
//                 enable: () => { }
//             };
//             cmp.form = <any>formSpy;

//             cmp.save();

//             expect(cmp.form.dirty).toBeFalsy();
//         }));
//         it('should not be possible to submit an invalid form', inject([PmodeService], (pmodeService: PmodeService) => {
//             pmode1.pmode.security = new ReceiveSecurity();
//             pmode1.pmode.security.decryption = new Decryption();
//             pmode1.pmode.security.decryption.encryption = 2;
//             store.setReceiving(pmode1);
//             let createReceivingSpy = spyOn(pmodeService, 'createReceiving').and.returnValue(Observable.of(true));
//             let updateReceivingSpy = spyOn(pmodeService, 'updateReceiving').and.returnValue(Observable.of(true));
//             let dialogSpy = spyOn(dialogService, 'incorrectForm');
//             cmp.form.get('pmode.security.decryption.encryption').setValue(2);
//             expect(cmp.form.valid).toBeFalsy();

//             cmp.save();

//             expect(createReceivingSpy).not.toHaveBeenCalled();
//             expect(updateReceivingSpy).not.toHaveBeenCalled();
//             expect(dialogSpy).toHaveBeenCalled();
//         }));
//     });
//     describe('reopen', () => {
//         it('should enable the form when the user switches back to the pmode', inject([], () => {
//             store.setReceiving(pmode1);
//             expect(cmp.form.enabled).toBeTruthy('Form should be enabled because of a setReceiving call (step1)');
//             cmp.init();
//             expect(cmp.form.enabled).toBeTruthy('Form should be enabled because init was called and setReceiving has a value!');
//         }));
//     });
// });
