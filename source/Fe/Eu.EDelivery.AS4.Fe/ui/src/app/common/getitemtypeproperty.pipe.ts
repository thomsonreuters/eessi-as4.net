import { PipeTransform, Pipe } from '@angular/core';

import { Property } from './../api/Property';
import { ItemType } from './../api/ItemType';

// tslint:disable-next-line:max-classes-per-file
@Pipe({
    name: 'getitemtypeproperty'
})
export class GetItemTypePropertyPipe implements PipeTransform {
    public transform(itemType: ItemType, property: string): Property | null {
        if (!!!itemType) {
            return null;
        }
        let prop = itemType.properties.find((search) => search.technicalName.toLowerCase() === property.toLowerCase());
        if (!!!prop) {
            return null;
        } else {
            return prop;
        }
    }
}

// tslint:disable-next-line:max-classes-per-file
@Pipe({
    name: 'gettype'
})
export class GetTypePipe implements PipeTransform {
    public transform(itemTypes: ItemType[], itemType: string, useTechnicalName: boolean = false): ItemType | undefined {
        if (!!!itemTypes) {
            return undefined;
        }
        if (useTechnicalName) {
            return itemTypes.find((search) => search.technicalName.toLowerCase() === itemType.toLowerCase());
        } else {
            return itemTypes.find((search) => search.name.toLowerCase() === itemType.toLowerCase());
        }
    }
}
