import { MessageFilter } from './message/message.filter';
import { URLSearchParams } from '@angular/http';
import { Params } from '@angular/router';
import moment from 'moment';

import { ISortDictionary } from './sortdictionary.interface';
import { getIsDate } from './inexception/isDate.decorator';

export class BaseFilter {
    public page: number = 1;
    public toUrlParams(): URLSearchParams {
        let params = new URLSearchParams();
        Object.keys(this).forEach((param) => {
            if (!!!this[param] || this[param].length === 0) {
                return;
            } else if (this[param] instanceof Date) {
                params.append(param, moment(this[param]).toISOString());
            } else {
                params.append(param, this[param]);
            }
        });
        return params;
    }
    public fromUrlParams(params: Params): BaseFilter {
        Object.keys(params).forEach((param) => {
            let isDate = !!getIsDate(this, param);
            if (isDate) {
                this[param] = moment(params[param]).toDate();
                return;
            }
            this[param] = params[param];
        });
        return this;
    }
    public sanitize(): any {
        let result = {};
        Object.keys(this).forEach((prop) => {
            if (!!!this[prop] || this[prop] === 'undefined') {
                return;
            }
            if (this[prop] instanceof Date) {
                result[prop] = moment(this[prop]).toISOString();
            } else {
                result[prop] = this[prop];
            }
        });
        return result;
    }
}
