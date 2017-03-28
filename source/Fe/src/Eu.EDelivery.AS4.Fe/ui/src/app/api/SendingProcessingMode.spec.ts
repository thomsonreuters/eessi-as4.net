import { FormBuilder } from '@angular/forms';
import { Observer } from 'rxjs/Observer';
import { Observable } from 'rxjs/Observable';
import {
    inject,
    TestBed
} from '@angular/core/testing';
import { Component } from '@angular/core';
import {
    BaseRequestOptions,
    ConnectionBackend,
    Http
} from '@angular/http';
import { MockBackend } from '@angular/http/testing';

import { SendingProcessingMode } from './SendingProcessingMode';
import { SendingProcessingModeForm } from './SendingProcessingModeForm';

describe('Receiving processing mode', () => {
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            FormBuilder
        ]
    }));
    it('should set default values when a new one is created', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let form = SendingProcessingModeForm.getForm(formBuilder, null);

        expect(form.get(SendingProcessingMode.FIELD_mep).value).toBe(0);
        expect(form.get(SendingProcessingMode.FIELD_mepBinding).value).toBe(1);
    }));
    it('should have the correct value when a value is used', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let data = new SendingProcessingMode();
        data.mep = 1;
        data.mepBinding = 0;
        let form = SendingProcessingModeForm.getForm(formBuilder, data);

        expect(form.get(SendingProcessingMode.FIELD_mep).value).toBe(1);
        expect(form.get(SendingProcessingMode.FIELD_mepBinding).value).toBe(0);
    }));
});

