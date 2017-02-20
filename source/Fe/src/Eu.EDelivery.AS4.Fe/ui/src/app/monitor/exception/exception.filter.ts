import { BaseFilter } from './../base.filter';
import { isDate } from './isDate.decorator';

export class ExceptionFilter extends BaseFilter {
    public operation: string;
    public ebmsRefToMessageId: string;
    @isDate()
    public modificationTimeFrom: Date;
    @isDate()
    public modificationTimeTo: Date;
    @isDate()
    public insertionTimeFrom: Date;
    @isDate()
    public insertionTimeTo: Date;
}
