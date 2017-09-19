import { Component, Input, ChangeDetectionStrategy, OnChanges, SimpleChanges } from '@angular/core';

import { RuntimeStore } from './../../settings/runtime.store';
import { RuntimeService } from './../../settings/runtime.service';

@Component({
    selector: 'as4-info',
    template: `<i *ngIf="tooltip" class="fa fa-info-circle" as4-tooltip="{{tooltip}}">`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class InfoComponent implements OnChanges {
    @Input() public tooltip: string;
    @Input() public runtimeTooltip: string;
    constructor(private _runtimeStore: RuntimeStore, private _runtimeService: RuntimeService) { }
    public ngOnChanges(changes: SimpleChanges) {
        if (!!this.runtimeTooltip && !!changes['runtimeTooltip'] && changes['runtimeTooltip'].previousValue !== changes['runtimeTooltip'].currentValue) {
            let search = this._runtimeService.getDescriptionByTechnicalName(this.runtimeTooltip);
            if (!!search) {
                this.tooltip = search.description;
            }
        }
    }
}
