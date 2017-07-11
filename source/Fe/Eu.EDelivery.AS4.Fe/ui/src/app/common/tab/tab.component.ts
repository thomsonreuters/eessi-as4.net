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
    OnDestroy,
    Output,
    EventEmitter
} from '@angular/core';

import { TabItemDirective } from './tabitem.directive';

@Component({
    selector: 'as4-tab',
    templateUrl: './tab.component.html',
    exportAs: 'as4-tab'
})
export class TabComponent implements AfterContentInit {
    @ContentChildren(TabItemDirective) public tabItems: QueryList<TabItemDirective>;
    public activeTab: TabItemDirective;
    public ngAfterContentInit() {
        let current = 0;
        this.tabItems.forEach((item) => item.tabId = current++);
        this.activeTab = this.tabItems.first;
    }
    public next() {
        const tabArray = this.tabItems.toArray();
        let index = tabArray.findIndex((tab) => tab === this.activeTab);
        if (index + 1 >= this.tabItems.length) {
            this.activeTab = tabArray[0];
        } else {
            this.activeTab = tabArray[++index];
        }
        this.updateActiveState();
    }
    public selectTab(tabItem: TabItemDirective) {
        this.activeTab = tabItem;
        this.updateActiveState();
    }
    private updateActiveState() {
        this.tabItems
            .forEach((item) => {
                if (item !== this.activeTab) {
                    item.setInactive();
                } else {
                    this.activeTab.setActive();
                }
            });
    }
}
