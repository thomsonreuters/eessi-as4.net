import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'contains'
})
export class ContainsPipe implements PipeTransform {
    public transform(value: any[], contains: any): boolean {
        return !!value.find((val) => val === contains);
    }
}
