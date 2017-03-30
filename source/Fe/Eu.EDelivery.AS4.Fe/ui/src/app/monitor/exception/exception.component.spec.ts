import { ActivatedRoute, Router } from '@angular/router';
import { Http, ConnectionBackend } from '@angular/http';
import {
    inject,
    fakeAsync,
    TestBed
} from '@angular/core/testing';

import { Exception } from './../../api/Messages/Exception';
import { As4ComponentsModule } from './../../common/as4components.module';
import { ExceptionService } from './exception.service';
import { ExceptionStore } from './exception.store';
import { ExceptionComponent } from './exception.component';

describe('InException component', () => {
    let messages: Exception[];
    let message1: Exception;

    // provide our implementations or mocks to the dependency injector
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            ExceptionComponent,
            ExceptionStore,
            { provide: ExceptionService, useClass: { getMessages() { } } },
            { provide: ActivatedRoute, useValue: { snapshot: { queryParams: {} } } }
        ],
        imports: [
            As4ComponentsModule
        ]
    }));
    beforeEach(() => {
        messages = new Array<Exception>();
        message1 = new Exception();
        message1.ebmsRefToMessageId = '100';
        messages.push(message1);
    });

    it('should be subscribed to the store', inject([ExceptionComponent, ExceptionStore], fakeAsync((cmp: ExceptionComponent, store: ExceptionStore) => {
        cmp.messages.skip(1).subscribe((result) => {
            expect(result.messages[0].ebmsRefToMessageId).toBe(message1.ebmsRefToMessageId);
        });
        store.update('messages', messages);
    })));
});
