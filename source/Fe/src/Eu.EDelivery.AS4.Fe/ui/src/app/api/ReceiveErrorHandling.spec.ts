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

import { ReceiveErrorHandling } from './ReceiveErrorHandling';
import { ReceiveErrorHandlingForm } from './ReceiveErrorHandlingForm';

describe('Receive error handling', () => {
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            FormBuilder
        ]
    }));
    it('should set default values when a new one is created', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let form = ReceiveErrorHandlingForm.getForm(formBuilder, null);

        expect(form.get(ReceiveErrorHandling.FIELD_useSoapFault).value).toBeFalsy();
        expect(form.get(ReceiveErrorHandling.FIELD_replyPattern).value).toBe(0);
        expect(+form.get(ReceiveErrorHandling.FIELD_responseHttpCode).value).toBe(200);
    }));
    it('should have the correct value when a value is used', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let data = new ReceiveErrorHandling();
        data.useSoapFault = true;
        data.replyPattern = 1;
        data.responseHttpCode = 500;
        let form = ReceiveErrorHandlingForm.getForm(formBuilder, data);

        expect(form.get(ReceiveErrorHandling.FIELD_useSoapFault).value).toBeTruthy();
        expect(form.get(ReceiveErrorHandling.FIELD_replyPattern).value).toBe(1);
        expect(+form.get(ReceiveErrorHandling.FIELD_responseHttpCode).value).toBe(500);
    }));
});

