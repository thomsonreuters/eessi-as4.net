import { BaseFilter } from './../base.filter';
import { isDate } from './isDate.decorator';

export class ExceptionFilter extends BaseFilter {
    public operation: string;
    public ebmsRefToMessageId: string;
    constructor(init?: Partial<ExceptionFilter>) {
        super();
        if (!!init) {
            Object.assign(this, init);
        }
        this.setDefaults();
    }
}
