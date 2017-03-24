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

import { TlsConfiguration } from './TlsConfiguration';
import { TlsConfigurationForm } from './TlsConfigurationForm';

describe('Tls configuration', () => {
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            FormBuilder
        ]
    }));
    it('should set default values when a new one is created', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let form = TlsConfigurationForm.getForm(formBuilder, null);

        expect(form.get(TlsConfiguration.FIELD_tlsVersion).value).toBe(3);
    }));
    it('should have the correct value when a value is used', inject([FormBuilder], (formBuilder: FormBuilder) => {
        let data = new TlsConfiguration();
        data.tlsVersion = 3;
        let form = TlsConfigurationForm.getForm(formBuilder, data);

        expect(form.get(TlsConfiguration.FIELD_tlsVersion).value).toBe(3);
    }));
});

