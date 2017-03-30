import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'todirection'
})
export class ToDirectionPipe implements PipeTransform {
    public transform(value: number): string {
         if (value === 0) {
             return 'Inbound';
         } else {
             return 'Outbound';
         }
    }
}
