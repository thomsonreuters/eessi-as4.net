import { IMessageService } from './../messageservice.interface';
import { MessageFilter } from './../message/message.filter';
import { RouterTestingModule } from '@angular/router/testing';
import { MESSAGESERVICETOKEN } from './../service.token';
import {
    inject,
    fakeAsync,
    TestBed,
    ComponentFixture,
    tick
} from '@angular/core/testing';
import {
    BaseRequestOptions,
    ConnectionBackend,
    ResponseOptions,
    Response,
    Http
} from '@angular/http';
import { ActivatedRoute, Router } from '@angular/router';
import { MockBackend } from '@angular/http/testing';
import { Subject } from 'rxjs/Subject';
import { AuthHttp } from 'angular2-jwt';
import { FilterComponent } from './filter.component';

let queryParams = new Subject<string>();

describe('Filter component', () => {
    // provide our implementations or mocks to the dependency injector
    beforeEach(() => TestBed.configureTestingModule({
        declarations: [
            FilterComponent
        ],
        imports: [
            RouterTestingModule.withRoutes([
                { path: 'test', component: FilterComponent }
            ])
        ],
        providers: [
            MockBackend,
            BaseRequestOptions,
            {
                provide: ActivatedRoute,
                useValue: {
                    queryParams,
                    snapshot: {
                        parent: null,
                        url: '',
                        queryParams: []
                    }
                }
            },
            {
                provide: AuthHttp,
                deps: [MockBackend, BaseRequestOptions],
                useFactory: (backend, options) => { return new Http(backend, options); }
            },
            {
                provide: MESSAGESERVICETOKEN,
                useValue: {
                    getMessages() { }
                }
            }
        ]
    }));
    let component: ComponentFixture<FilterComponent>;
    let activatedRoute: ActivatedRoute;
    let router: Router;
    let filter: MessageFilter;
    let service: IMessageService;
    beforeEach(inject([ActivatedRoute, Router, MESSAGESERVICETOKEN], fakeAsync((_activatedRoute: ActivatedRoute, _router: Router, _service: IMessageService) => {
        component = TestBed.createComponent(FilterComponent);
        activatedRoute = _activatedRoute;
        router = _router;
        service = _service;
        filter = new MessageFilter();
        component.componentInstance.filter = filter;
        advance();
    })));
    afterEach(() => component.destroy());
    it('search must be called when the querystring changes', inject([], fakeAsync(() => {
        let routerSpy = spyOn(router, 'navigate');
        let serviceSpy = spyOn(service, 'getMessages');
        queryParams.next('nextParams');
        advance();

        expect(routerSpy).not.toHaveBeenCalled();
        expect(serviceSpy).toHaveBeenCalled();
    })));
    it('onInit calls executeServicecall', inject([], () => {
        let executeServiceCallSpy = spyOn(component.componentInstance, 'executeServiceCall');
        component.componentInstance.ngOnInit();
        expect(executeServiceCallSpy).toHaveBeenCalledWith();
        expect(component.componentInstance.outFilter).toBeDefined();
    }));
    it('search calls router.navigate when called without parameters', () => {
        let routerSpy = spyOn(router, 'navigate');
        component.componentInstance.search();

        expect(routerSpy).toHaveBeenCalled();
    });
    function advance() {
        tick();
        component.detectChanges();
    }
});
