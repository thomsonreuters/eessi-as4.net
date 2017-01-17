import { FormGroup, FormBuilder } from '@angular/forms';
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

import { getRawFormValues } from './getRawFormValues';

describe('getRawValues', () => {
    let form: FormGroup;
    beforeEach(() => {
        let formBuilder = new FormBuilder();
        form = formBuilder.group({
            name: ['name'],
            pmode: formBuilder.group({
                pmodeName: ['pmodeName'],
                test: formBuilder.group({
                    id: formBuilder.control('id')
                })
            })
        });

        form.disable();
        form.get('name').disable();
        form.get('pmode.pmodeName').disable();
    });

    it('should return values of all form controls including disabled when used with getRawFormValues', () => {
        let result = getRawFormValues(form);

        expect(JSON.stringify(result)).toBe(JSON.stringify({
            name: 'name',
            pmode: {
                pmodeName: 'pmodeName',
                test: {
                    id: 'id'
                }
            }
        }));
    });
});
