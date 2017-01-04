import { Component, OnInit, ContentChildren, ViewChildren, QueryList, ViewEncapsulation, ChangeDetectionStrategy } from '@angular/core';

import { TabItemComponent } from './tabitem.component';

@Component({
    selector: 'as4-tab',
    templateUrl: './tab.component.html'
})
export class TabComponent {
    @ContentChildren(TabItemComponent) tabItems: QueryList<TabItemComponent>;
    constructor() {
    }

    ngAfterContentInit() {
        let current = 0;
        this.tabItems.forEach(item => item.tabId = current++);
    }
}
