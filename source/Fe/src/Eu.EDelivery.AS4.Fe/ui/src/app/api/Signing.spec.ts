import { PushConfiguration } from './PushConfiguration';
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

import { Signing } from './Signing';

describe('Tls configuration', () => {
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            FormBuilder
        ]
    }));
    it('should set default values when a new one is created', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let form = Signing.getForm(formBuilder, null);

        expect(form.get(Signing.FIELD_algorithm).value).toBe('http://www.w3.org/2001/04/xmldsig-more#rsa-sha256');
        expect(form.get(Signing.FIELD_hashFunction).value).toBe('http://www.w3.org/2001/04/xmlenc#sha256');
    }));
    it('should have the correct value when a value is used', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let data = new Signing();
        data.algorithm = 'test';
        data.hashFunction = 'test2';
        let form = Signing.getForm(formBuilder, data);

        expect(form.get(Signing.FIELD_algorithm).value).toBe('test');
        expect(form.get(Signing.FIELD_hashFunction).value).toBe('test2');
    }));
});

