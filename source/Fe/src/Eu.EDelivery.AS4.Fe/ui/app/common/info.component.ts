import { enableDebugTools } from '@angular/platform-browser';
import { Component, Input } from '@angular/core';

@Component({
    selector: 'as4-info',
    template: `<i *ngIf="tooltip" class="fa fa-info-circle" as4-tooltip="{{tooltip}}">`
})
export class InfoComponent {
    @Input() tooltip: string;
}