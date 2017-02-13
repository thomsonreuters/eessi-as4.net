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

import { SpinnerService } from './spinner.service';

describe('spinner service', () => {
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            SpinnerService
        ]
    }));
    describe('when show is called', () => {
        it('should publish true', inject([SpinnerService], (service: SpinnerService) => {
            let subscribe = service.changes
                .skip(1)
                .subscribe(result => {
                    expect(result).toBeTruthy();
                });

            service.show();
        }));
    });
    describe('when hide is called', () => {
        it('should publish false after it published true', inject([SpinnerService], (service: SpinnerService) => {
            let subscribe = service.changes
                .skip(2)
                .subscribe(result => expect(result).toBeFalsy());

            service.show();
            service.hide();
        }));
    });
});
