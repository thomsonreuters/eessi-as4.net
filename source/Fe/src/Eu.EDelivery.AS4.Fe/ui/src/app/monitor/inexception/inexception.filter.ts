import { BaseFilter } from './../base.filter';
import { isDate } from './isDate.decorator';

export class InExceptionFilter extends BaseFilter {
    public id: number;
    public operation: number;
    public exceptionType: number;
    public operationMethod: string;
    public ebmsRefToMessageId: string;
    public pmode: string;
    @isDate()
    public modificationTimeFrom: Date;
    @isDate()
    public modificationTimeTo: Date;
    @isDate()
    public insertionTimeFrom: Date;
    @isDate()
    public insertionTimeTo: Date;
}
