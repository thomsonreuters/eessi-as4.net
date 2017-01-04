import { SigningVerification } from './SigningVerification';
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

import { ReceiveReceiptHandling } from './ReceiveReceiptHandling';

describe('Receive receipt handling', () => {
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            FormBuilder
        ]
    }));
    it('should set default values when a new one is created', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let form = ReceiveReceiptHandling.getForm(formBuilder, null);

        expect(form.get(ReceiveReceiptHandling.FIELD_useNNRFormat).value).toBeFalsy();
        expect(form.get(ReceiveReceiptHandling.FIELD_replyPattern).value).toBe(0);
    }));
    it('should have the correct value when a value is used', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let data = new ReceiveReceiptHandling();
        data.replyPattern = 1;
        data.useNNRFormat = true;
        let form = ReceiveReceiptHandling.getForm(formBuilder, data);

        expect(form.get(ReceiveReceiptHandling.FIELD_replyPattern).value).toBeTruthy();
        expect(form.get(ReceiveReceiptHandling.FIELD_replyPattern).value).toBe(1);
    }));
});

