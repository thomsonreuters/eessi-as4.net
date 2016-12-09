import { ModalService } from './common/modal.service';
import {
    inject,
    TestBed
} from '@angular/core/testing';

// Load the implementations that should be tested
import { AppComponent } from './app.component';
import { AppState } from './app.service';
import { AuthenticationStore } from './authentication/authentication.service';
import { RuntimeService, IRuntimeService } from './settings/runtime.service';
import { RuntimeServiceMock } from './settings/runtime.service.mock';

describe('App', () => {

    // provide our implementations or mocks to the dependency injector
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            AppState,
            AppComponent,
            AuthenticationStore,
            ModalService,
            { provide: RuntimeService, useClass: RuntimeServiceMock }
        ]
    }));

    it('should default to be not logged in', inject([AppComponent], (app: AppComponent) => {
        expect(app.isLoggedIn).toEqual(false);
    }));

    it('should be logged in when authenticationStore publishes', inject([AppComponent, AuthenticationStore, RuntimeService], (app: AppComponent, store: AuthenticationStore, runtimeService: RuntimeService) => {
        // Setup
        spyOn(runtimeService, 'getAll');

        // Act
        store.setState({ loggedin: true });

        // Assert
        expect(app.isLoggedIn).toBeTruthy();
        expect(runtimeService.getAll).toHaveBeenCalled();
    }));

    it('should not call runtimeservices on failed login', inject([AppComponent, AuthenticationStore, RuntimeService], (app: AppComponent, store: AuthenticationStore, runtimeService: RuntimeService) => {
        // Setup
        let spy1 = spyOn(runtimeService, 'getReceivers');
        let spy2 = spyOn(runtimeService, 'getCertificateRepositories');
        let spy3 = spyOn(runtimeService, 'getSteps');
        let spy4 = spyOn(runtimeService, 'getTransformers');

        // Act 
        store.setState({ loggedin: false });

        // Assert
        expect(app.isLoggedIn).toBeFalsy();
        expect(spy1.calls.any()).toBeFalsy();
        expect(spy2.calls.any()).toBeFalsy();
        expect(spy3.calls.any()).toBeFalsy();
        expect(spy4.calls.any()).toBeFalsy();
    }));
});
