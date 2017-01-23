import {
    Component,
    OnInit,
    ContentChildren,
    ViewChildren,
    QueryList,
    ViewEncapsulation,
    ChangeDetectionStrategy,
    AfterContentInit
} from '@angular/core';

import { TabItemComponent } from './tabitem.component';

@Component({
    selector: 'as4-tab',
    templateUrl: './tab.component.html'
})
export class TabComponent implements AfterContentInit {
    @ContentChildren(TabItemComponent) public tabItems: QueryList<TabItemComponent>;
    public ngAfterContentInit() {
        let current = 0;
        this.tabItems.forEach((item) => item.tabId = current++);
    }
}
