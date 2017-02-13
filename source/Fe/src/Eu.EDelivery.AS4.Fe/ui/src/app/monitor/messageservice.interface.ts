import { OpaqueToken } from '@angular/core';
import { BaseFilter } from './base.filter';

export interface IMessageService {
    getMessages(filter: BaseFilter): any[];
}

export const MESSAGESERVICETOKEN = new OpaqueToken('MESSAGESERVICE');