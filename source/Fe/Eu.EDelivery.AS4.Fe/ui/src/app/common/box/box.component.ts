import { Component, ElementRef, Input, OnInit, Renderer } from '@angular/core';

@Component({
    selector: 'as4-box',
    templateUrl: './box.component.html'
})
export class BoxComponent implements OnInit {
    @Input() public title: string;
    @Input() public collapsed: boolean = false;
    @Input() public fullWidth: boolean = false;
    @Input() public collapsible: boolean = false;
    @Input() public showTitle: boolean = false;
    constructor(private elementRef: ElementRef, private renderer: Renderer) {}

    public ngOnInit() {
        if (this.fullWidth) {
            this.renderer.setElementStyle(this.elementRef.nativeElement, 'width', '100%');
            this.renderer.setElementStyle(this.elementRef.nativeElement, 'margin-right', '32px');
        }
    }
}
