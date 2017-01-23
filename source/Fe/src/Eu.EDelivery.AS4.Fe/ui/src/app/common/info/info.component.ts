import { enableDebugTools } from '@angular/platform-browser';
import { Component, Input, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'as4-info',
    template: `<i *ngIf="tooltip" class="fa fa-info-circle" as4-tooltip="{{tooltip}}">`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class InfoComponent {
    @Input() public tooltip: string;
}
