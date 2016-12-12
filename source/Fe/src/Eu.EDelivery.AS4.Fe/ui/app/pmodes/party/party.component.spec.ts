import { ModalService } from './../../common/modal.service';
import { DialogService } from './../../common/dialog.service';
import { By } from '@angular/platform-browser/src/dom/debug/by';
import { ReactiveFormsModule, FormBuilder, FormArray } from '@angular/forms';
import { Observable } from 'rxjs';
import {
    inject,
    TestBed,
    async,
    ComponentFixture
} from '@angular/core/testing';
import { Component } from '@angular/core';
import {
    BaseRequestOptions,
    ConnectionBackend,
    Http
} from '@angular/http';
import { MockBackend } from '@angular/http/testing';

import { PartyComponent } from './party.component';
import { Party } from './../../api/Party';

describe('party', () => {
    let fixture: ComponentFixture<PartyComponent>;
    let instance: PartyComponent;
    let party: Party;
    beforeEach(async(() => TestBed.configureTestingModule({
        declarations: [
            PartyComponent
        ],
        imports: [
            ReactiveFormsModule
        ],
        providers: [
            FormBuilder,
            DialogService,
            ModalService
        ]
    }).compileComponents()));
    beforeEach(() => {
        fixture = TestBed.createComponent(PartyComponent);
        instance = fixture.componentInstance;

        party = new Party();
    });
    afterEach(() => fixture.destroy());

    describe('add', () => {
        it('should add new item to the form', inject([FormBuilder], (formBuilder: FormBuilder) => {
            instance.group = Party.getForm(formBuilder, party);
            fixture.debugElement.query(By.css('.add-button')).nativeElement.click();
            fixture.detectChanges();

            let form = <FormArray>instance.group.controls[Party.FIELD_partyIds];
            expect(form.length).toBe(1);
        }));
    });
    describe('remove', () => {
        it('should remove the partyId', inject([FormBuilder, DialogService], (formBuilder: FormBuilder, dialogService: DialogService) => {
            let dialogSpy = spyOn(dialogService, 'deleteConfirm').and.returnValue(Observable.of(true));
            instance.group = Party.getForm(formBuilder, party);
            instance.group.markAsPristine();
            expect(instance.group.dirty).toBeFalsy();
            fixture.debugElement.query(By.css('.add-button')).nativeElement.click();
            fixture.detectChanges();

            let remove = fixture.debugElement.query(By.css('.remove-button')).nativeElement.click();

            let form = <FormArray>instance.group.controls[Party.FIELD_partyIds];
            expect(form.length).toBe(0);
            expect(instance.group.dirty).toBeTruthy();
        }));
        it('should ask the user for confirmation', inject([DialogService, FormBuilder], (dialogService: DialogService, formBuilder: FormBuilder) => {
            instance.group = Party.getForm(formBuilder, party);
            let dialogSpy = spyOn(dialogService, 'deleteConfirm').and.returnValue(Observable.of(false));

            instance.removeParty(0);

            expect(dialogSpy).toHaveBeenCalled();
        }));
    });
});
