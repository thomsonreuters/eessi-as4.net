import { By } from '@angular/platform-browser/src/dom/debug/by';
import { Observable } from 'rxjs';
import {
    inject,
    TestBed,
    ComponentFixture
} from '@angular/core/testing';
import { Component } from '@angular/core';
import {
    BaseRequestOptions,
    ConnectionBackend,
    Http
} from '@angular/http';
import { MockBackend } from '@angular/http/testing';

import { CrudButtonsComponent } from './crudbuttons.component';

describe('crudbuttons', () => {
    beforeEach(() => TestBed.configureTestingModule({
        declarations: [
            CrudButtonsComponent
        ]
    }));
    function getInstance() {
        let cmp = TestBed.createComponent(CrudButtonsComponent);
        let instance = cmp.componentInstance;
        instance.form = <any>{
            dirty: false
        };
        return cmp;
    }
    function testButton(name: string) {
        let instance = getInstance();
        let eventSpy = spyOn(instance.componentInstance[name], 'emit');

        instance.debugElement.query(By.css(`.${name}-button`)).nativeElement.click();
        instance.detectChanges();

        expect(instance.componentInstance[name].emit).toHaveBeenCalled();
    }

    it('should emit events when button is clicked', () => {
        testButton('rename');
        testButton('add');
        testButton('save');
        testButton('delete');
        testButton('reset');
    });
    describe('delete and rename', () => {
        it('should have the disabled attribute when current is not defined', () => {
            let instance = getInstance();

            let renameButton = instance.debugElement.query(By.css('.rename-button'));
            let deleteButton = instance.debugElement.query(By.css('.delete-button'));
            instance.detectChanges();

            expect(renameButton.nativeElement.attributes['disabled']).toBeDefined();
            expect(deleteButton.nativeElement.attributes['disabled']).toBeDefined();
        });
        it('should not have the disabled attribute when current is defined', () => {
            let instance = getInstance();
            instance.componentInstance.current = 'test';

            let renameButton = instance.debugElement.query(By.css('.rename-button'));
            let deleteButton = instance.debugElement.query(By.css('.delete-button'));
            instance.detectChanges();

            expect(renameButton.nativeElement.attributes['disabled']).toBeUndefined();
            expect(deleteButton.nativeElement.attributes['disabled']).toBeUndefined();
        });
    });
    describe('add', () => {
        it('should be disabled when in new mode', () => {
            let instance = getInstance();
            instance.componentInstance.isNewMode = true;

            let addButton = instance.debugElement.query(By.css('.add-button'));
            instance.detectChanges();

            expect(addButton.nativeElement.attributes['disabled']).toBeDefined();
        });
        it('should be enabled when not in new mode', () => {
            let instance = getInstance();
            instance.componentInstance.isNewMode = false;

            let addButton = instance.debugElement.query(By.css('.add-button'));
            instance.detectChanges();

            expect(addButton.nativeElement.attributes['disabled']).toBeUndefined();
        });
    });
    describe('form dirty state', () => {
        it('should disable save when form is not dirty', () => {
            let instance = getInstance();
            instance.componentInstance.form.dirty = false;

            let saveButton = instance.debugElement.query(By.css('.save-button'));
            let resetButton = instance.debugElement.query(By.css('.reset-button'));
            instance.detectChanges();

            expect(saveButton.nativeNode.attributes['disabled']).toBeDefined();
            expect(resetButton.nativeNode.attributes['disabled']).toBeDefined();
        });
        it('should enable save when form is dirty', () => {
            let instance = getInstance();
            instance.componentInstance.form.dirty = true;

            let saveButton = instance.debugElement.query(By.css('.save-button'));
            let resetButton = instance.debugElement.query(By.css('.reset-button'));
            instance.detectChanges();

            expect(saveButton.nativeNode.attributes['disabled']).toBeUndefined();
            expect(resetButton.nativeNode.attributes['disabled']).toBeUndefined();
        });
    });
    describe('isNewMode', () => {
        it('should set correct buttons enabled', () => {
            let instance = getInstance();
            instance.componentInstance.isNewMode = true;
            instance.detectChanges();

            let saveButton = instance.debugElement.query(By.css('.save-button'));
            let resetButton = instance.debugElement.query(By.css('.reset-button'));
            instance.detectChanges();

            expect(saveButton.nativeNode.attributes['disabled']).toBeUndefined();
            expect(resetButton.nativeNode.attributes['disabled']).toBeUndefined();
        });
    });
});
