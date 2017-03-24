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

import { ReceiveSecurity } from './ReceiveSecurity';
import { ReceiveSecurityForm } from './ReceiveSecurityForm';

describe('Receive security', () => {
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            FormBuilder
        ]
    }));
    it('should set default values when a new one is created', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let form = ReceiveSecurityForm.getForm(formBuilder, null);

        expect(form.get('signingVerification.signature').value).toBe(0);
    }));
    it('should have the correct value when a value is used', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let data = new ReceiveSecurity();
        data.signingVerification = new SigningVerification();
        data.signingVerification.signature = 2;
        let form = ReceiveSecurityForm.getForm(formBuilder, data);

        expect(form.get('signingVerification.signature').value).toBe(2);
    }));
});

