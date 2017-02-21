import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'tonumber'
})
export class ToNumberPipe implements PipeTransform {
    public transform(value: string): number {
         return +value;
    }
}
