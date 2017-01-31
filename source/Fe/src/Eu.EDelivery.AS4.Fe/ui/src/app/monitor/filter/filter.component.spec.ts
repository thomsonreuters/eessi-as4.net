import { ActivatedRoute, Router } from '@angular/router';
import { Http, ConnectionBackend } from '@angular/http';
import {
    inject,
    fakeAsync,
    TestBed
} from '@angular/core/testing';

import { FilterComponent } from './filter.component';
import { MESSAGESERVICETOKEN, IMessageService } from '../messageservice.interface';

const router = { parent: undefined, snapshot: { queryParams: { name: 'test' }, url: 'test' } };

describe('filter', () => {
    // provide our implementations or mocks to the dependency injector
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            FilterComponent,
            { provide: MESSAGESERVICETOKEN, useValue: { getMessages() { } } },
            { provide: ActivatedRoute, useValue: router },
            { provide: Router, useValue: { navigate() { } } }
        ]
    }));
    describe('ngOnInit', () => {
        it('should call messageservice.getMessages', inject([FilterComponent, MESSAGESERVICETOKEN], (cmp: FilterComponent, service: IMessageService) => {
            let serviceSpy = spyOn(service, 'getMessages');

            cmp.ngOnInit();

            expect(serviceSpy).toHaveBeenCalled();
        }));
        it('should receive the queryParams from the activatedRoute', inject([FilterComponent, MESSAGESERVICETOKEN, ActivatedRoute], fakeAsync((cmp: FilterComponent, service: IMessageService, route: ActivatedRoute) => {
            (<any>cmp.filter).name = 'test';
            let serviceSpy = spyOn(service, 'getMessages');

            cmp.ngOnInit();

            expect(serviceSpy).toHaveBeenCalledWith(cmp.filter);
        })));
    });
    describe('search', () => {
        it('should set queryParams and call service.getMessages', inject([FilterComponent, MESSAGESERVICETOKEN, Router], (cmp: FilterComponent, service: IMessageService, router: Router) => {
            let routerSpy = spyOn(router, 'navigate');
            let serviceSpy = spyOn(service, 'getMessages');
            cmp.onSearch.subscribe((result) => expect(result).toBeDefined());
            cmp.search();

            expect(routerSpy).toHaveBeenCalledWith(['test'], { queryParams: cmp.filter });
            expect(serviceSpy).toHaveBeenCalledWith(cmp.filter);
        }));
    });
});
