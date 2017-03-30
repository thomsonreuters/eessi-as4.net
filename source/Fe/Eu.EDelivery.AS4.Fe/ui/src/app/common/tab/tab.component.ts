import { Subscription } from 'rxjs/Subscription';
import {
    Component,
    OnInit,
    ContentChildren,
    ViewChildren,
    QueryList,
    ViewEncapsulation,
    ChangeDetectionStrategy,
    AfterContentInit,
    OnDestroy
} from '@angular/core';

import { TabItemDirective } from './tabitem.directive';

@Component({
    selector: 'as4-tab',
    templateUrl: './tab.component.html'
})
export class TabComponent implements AfterContentInit {
    @ContentChildren(TabItemDirective) public tabItems: QueryList<TabItemDirective>;
    public activeTab: TabItemDirective;
    public ngAfterContentInit() {
        let current = 0;
        this.tabItems.forEach((item) => item.tabId = current++);
        this.activeTab = this.tabItems.first;
    }
}
