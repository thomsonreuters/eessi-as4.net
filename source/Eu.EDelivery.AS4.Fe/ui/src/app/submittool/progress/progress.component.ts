import { Component, Input, ChangeDetectionStrategy, ViewChild, ElementRef, Renderer, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'as4-progress, [as4-progress]',
    templateUrl: 'progress.component.html',
    styles: [`
        .progress-sm {
            margin-bottom: 0 !important;
        }
    `],
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class ProgressComponent {
    @Input() public set progress(value: number) {
        this._renderer.setElementStyle(this.progressEle.nativeElement, 'width', `${value}%`);
    }
    @ViewChild('progressEle') public progressEle: ElementRef;
    constructor(private _renderer: Renderer, private _changeDetector: ChangeDetectorRef) {
        this._changeDetector.detach();
    }
}
