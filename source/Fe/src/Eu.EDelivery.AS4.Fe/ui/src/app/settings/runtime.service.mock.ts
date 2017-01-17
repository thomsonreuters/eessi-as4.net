import { IRuntimeService } from './runtime.service';

export class RuntimeServiceMock implements IRuntimeService {
    getReceivers() { }
    getSteps() { }
    getTransformers() { }
    getCertificateRepositories() { }
    getAll() { }
}
