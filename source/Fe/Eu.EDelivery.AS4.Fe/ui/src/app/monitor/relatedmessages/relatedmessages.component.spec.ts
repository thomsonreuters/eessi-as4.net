import { ToDirectionPipe } from './../todirection.pipe';
import { RouterTestingModule } from '@angular/router/testing';
import { As4ComponentsModule } from './../../common/as4components.module';
import { OpaqueToken } from '@angular/core';
import { MessageService } from './../message/message.service';
import { MessageFilter } from './../message/message.filter';
import { ExceptionService } from './../exception/exception.service';
import {
    inject,
    TestBed,
    ComponentFixture
} from '@angular/core/testing';
import {
    BaseRequestOptions,
    ConnectionBackend,
    ResponseOptions,
    Response,
    Http
} from '@angular/http';
import { MockBackend } from '@angular/http/testing';
import { AuthHttp } from 'angular2-jwt';

import { RelatedMessagesComponent } from './relatedmessages.component';
import { IMessageService } from './../messageservice.interface';
import { MESSAGESERVICETOKEN } from './../service.token';
import { MessageStore } from './../message/message.store';

let messageService: any = {
    getMessages() { },
    getRelatedMessages() { }
};

let SERVICETOKEN = new OpaqueToken('MOCKTOKEN');

describe('Related messages', () => {
    // provide our implementations or mocks to the dependency injector
    beforeEach(() => TestBed.configureTestingModule({
        declarations: [
            RelatedMessagesComponent,
            ToDirectionPipe
        ],
        imports: [
            As4ComponentsModule,
            RouterTestingModule
        ],
        providers: [
            MockBackend,
            BaseRequestOptions,
            MessageStore,
            {
                provide: SERVICETOKEN,
                useValue: messageService
            },
            {
                provide: MESSAGESERVICETOKEN,
                useExisting: SERVICETOKEN
            },
            {
                provide: AuthHttp,
                deps: [MockBackend, BaseRequestOptions],
                useFactory: (backend, options) => { return new Http(backend, options); }
            }
        ]
    }).overrideComponent(RelatedMessagesComponent, {
        set: {
            providers: [
                {
                    provide: MESSAGESERVICETOKEN, useExisting: SERVICETOKEN
                },
                {
                    provide: MessageStore, useValue: MessageStore
                }
            ]
        }
    }));
    let component: ComponentFixture<RelatedMessagesComponent>;
    let service: IMessageService;
    let direction: number = 0;
    let messageId: string = 'messageId';
    beforeEach(() => {
        component = TestBed.createComponent(RelatedMessagesComponent);
    });
    beforeEach(inject([MESSAGESERVICETOKEN], (_service: IMessageService) => {
        service = _service;

        component.componentInstance.direction = direction;
        component.componentInstance.messageId = messageId;
    }));
    afterEach(() => {
        component.destroy();
    });
    it('should not load messages when messageId input is empty', inject([], () => {
        let serviceSpy = spyOn(service, 'getMessages');
        component.componentInstance.messageId = null;

        component.componentInstance.ngOnInit();

        expect(serviceSpy).not.toHaveBeenCalled();
    }));
    it('calls service with the messageid', inject([], () => {
        let serviceSpy = spyOn(service, 'getRelatedMessages');
        component.componentInstance.ngOnInit();
        expect(serviceSpy).toHaveBeenCalledWith(direction, messageId);
    }));
});
