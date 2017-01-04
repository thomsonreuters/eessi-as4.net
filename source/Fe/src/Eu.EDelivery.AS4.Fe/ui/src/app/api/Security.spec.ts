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

import { Security } from './Security';

describe('Security', () => {
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            FormBuilder
        ]
    }));
    it('should set default values when a new one is created', inject([FormBuilder], (formBuilder: FormBuilder) => {
        // let form = Security.getForm(formBuilder, null);

        // expect(form.get(Security.FIELD_signing).value).toBe(0);
    }));
    it('should have the correct value when a value is used', inject([FormBuilder], (formBuilder: FormBuilder) => {
        //         let data = new Security();
        //         data.encryption
        //         let form = Security.getForm(formBuilder, data);

        // expect(form.get(Security.FIELD_signing).value).toBe(0);
    }));
});

