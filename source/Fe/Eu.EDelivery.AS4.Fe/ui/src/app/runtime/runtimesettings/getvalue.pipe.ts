import { FormArray, FormGroup } from '@angular/forms';
import { Pipe, PipeTransform } from '@angular/core';

import { ItemType } from './../../api/ItemType';
import { Property } from './../../api/Property';

@Pipe({
    name: 'getvalue'
})
export class GetValuePipe implements PipeTransform {
    public transform(type: ItemType, property: string): Property | undefined {
        return type.properties.find((prop) => prop.technicalName === property);
    }
}

@Pipe({
    name: 'getkeys'
})
export class GetKeysPipe implements PipeTransform {
    public transform(obj: FormGroup[]): string[] | null {
        if (!!!obj) {
            return null;
        }

        let result = obj.map((control => Object.keys(control.controls)));
        return result.reduce((a, b) => a.concat(b));
    }
}
