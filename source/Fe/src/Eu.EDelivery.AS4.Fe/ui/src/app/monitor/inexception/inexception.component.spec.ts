import { ActivatedRoute, Router } from '@angular/router';
import { InException } from './../../api/Messages/InException';
import { Http, ConnectionBackend } from '@angular/http';
import {
    inject,
    fakeAsync,
    TestBed
} from '@angular/core/testing';

import { As4ComponentsModule } from './../../common/as4components.module';
import { InExceptionService } from './inexception.service';
import { InExceptionStore } from './inexception.store';
import { InExceptionComponent } from './inexception.component';

describe('InException component', () => {
    let messages: InException[];
    let message1: InException;

    // provide our implementations or mocks to the dependency injector
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            InExceptionComponent,
            InExceptionStore,
            { provide: InExceptionService, useClass: { getMessages() { }} },
            { provide: ActivatedRoute, useValue: { snapshot: { queryParams: {} } } },
            { provide: Router, useClass: { } }
        ],
        imports: [
            As4ComponentsModule
        ]
    }));
    beforeEach(() => {
        messages = new Array<InException>();
        message1 = new InException();
        message1.id = 100;
        messages.push(message1);
    });

    it('should be subscribed to the store', inject([InExceptionComponent, InExceptionStore], fakeAsync((cmp: InExceptionComponent, store: InExceptionStore) => {
        cmp.messages.skip(1).subscribe((result) => {
            expect(result.messages[0].id).toBe(message1.id);
        });
        store.update('messages', messages);
    })));
});
