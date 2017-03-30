import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'toNumberArray'
})
export class ToNumberArrayPipe implements PipeTransform {
    public transform(value: number): number[] {
        let result = new Array<number>();
        for (let i = 0; i < value; i++) {
            result.push(i + 1);
        }
        return result;
    }
}
