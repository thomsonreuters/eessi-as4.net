import { IRuntimeService } from './runtime.service';

export class RuntimeServiceMock implements IRuntimeService {
    public getReceivers() { }
    public getSteps() { }
    public getTransformers() { }
    public getCertificateRepositories() { }
    public getAll() { }
}
