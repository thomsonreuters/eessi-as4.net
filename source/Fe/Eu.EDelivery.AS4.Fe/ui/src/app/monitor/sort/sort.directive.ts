import { Directive, ElementRef, OnInit, Input, HostListener } from '@angular/core';

import { BaseFilter } from './../base.filter';
import { SortOrder } from './../sortorder.enum';

@Directive({
    selector: 'th[sort]'
})
export class SortDirective {
    @Input() public field: string;
    @Input() public filter: BaseFilter;
    @HostListener('click')
    public onClick() {
        this.filter[this.field] = SortOrder.ascending;
    }
}
