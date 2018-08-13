import { Pipe, PipeTransform } from '@angular/core';
import moment from 'moment';

@Pipe({
    name: 'todate'
})
export class ToDatePipe implements PipeTransform {
    public transform(value: Date): string {
         return moment(value).format('YYYY/MM/DD hh:mm:ss.SS');
    }
}
