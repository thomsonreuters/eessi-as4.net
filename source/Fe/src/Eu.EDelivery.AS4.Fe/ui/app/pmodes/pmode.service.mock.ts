import { IPmodeService } from './pmode.service';

export class PmodeServiceMock implements IPmodeService {
    getAllReceiving() { }
    getReceiving() { }
    getAllSending() { }
    getSending() { }
    deleteReceiving(name: string) { }
    deleteSending(name: string) { }
}
