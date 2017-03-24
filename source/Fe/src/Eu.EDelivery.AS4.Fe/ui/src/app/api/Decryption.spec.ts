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

import { Decryption } from './Decryption';
import { DecryptionForm } from './DecryptionForm';

describe('Decryption', () => {
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            FormBuilder
        ]
    }));
    it('should set default values when a new one is created', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let form = DecryptionForm.getForm(formBuilder, null);

        expect(form.get(Decryption.FIELD_encryption).value).toBe(0);
    }));
    it('should have the correct value when a value is used', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let data = new Decryption();
        data.encryption = 3;
        let form = DecryptionForm.getForm(formBuilder, data);

        expect(form.get(Decryption.FIELD_encryption).value).toBe(3);
    }));
});

