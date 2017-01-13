import { PmodeStore } from '../pmode.store';
import { ActivatedRoute } from '@angular/router';
import { PmodeSelectComponent } from './pmodeselect.component';
import { Observable } from 'rxjs';
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

import { AuthHttp } from 'angular2-jwt';
import { PmodeService } from '../pmode.service';

describe('pmode select component', () => {
    let receivingNames: Array<string>;
    let sendingNames: Array<string>;
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            PmodeSelectComponent,
            {
                provide: PmodeService, useValue: {
                    getAllReceiving() { },
                    getAllSending() { }
                }
            },
            PmodeStore
        ]
    }));
    beforeEach(() => {
        receivingNames = new Array<string>();
        receivingNames.push('receivingPmode');
        sendingNames = new Array<string>();
        sendingNames.push('sendingPmode');
    });
    it('Should load the pmodes from the correct store', inject([PmodeSelectComponent, PmodeService, PmodeStore], (cmp: PmodeSelectComponent, pmodeService: PmodeService, store: PmodeStore) => {
        // Receiving
        cmp.mode = 'receiving';
        store.setState({
            Receiving: null,
            Sending: null,
            ReceivingNames: receivingNames,
            SendingNames: null
        });
        cmp.ngOnInit();
        expect(cmp.pmodes).toBe(receivingNames);

        // Sending
        cmp.mode = 'sending';
        store.setState({
            Receiving: null,
            Sending: null,
            ReceivingNames: null,
            SendingNames: sendingNames
        });
    }));
    it('should throw exception when no mode has been supplied', () => {
        expect(() => new PmodeSelectComponent(null, null).ngOnInit()).toThrowError('Mode should be supplied');
    });
    it('should set selectedPmode to the selected mode', inject([PmodeSelectComponent], (cmp: PmodeSelectComponent) => {
        // Setup
        let isRegisterOnChangeCalled: boolean;
        cmp.registerOnChange(() => isRegisterOnChangeCalled = true);
        cmp.pmodes = sendingNames;

        // Act
        cmp.selectPmode(cmp.pmodes[0]);

        // Assert
        expect(cmp.selectedPmode).toBe(cmp.pmodes[0]);
        expect(isRegisterOnChangeCalled).toBeTruthy();
    }));
});
