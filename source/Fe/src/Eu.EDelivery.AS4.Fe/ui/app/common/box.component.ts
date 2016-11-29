import { Component, OnInit, Input, ElementRef, Renderer } from '@angular/core';

@Component({
    selector: 'as4-box',
    templateUrl: './box.component.html'
})
export class BoxComponent implements OnInit {
    @Input() title: string;
    @Input() collapsed: boolean = false;
    @Input() fullWidth: boolean = false;
    constructor(private elementRef: ElementRef, private renderer: Renderer) {

    }

    ngOnInit() {
        if (this.fullWidth) {
            this.renderer.setElementStyle(this.elementRef.nativeElement, 'width', '100%');
            this.renderer.setElementStyle(this.elementRef.nativeElement, 'margin-right', '32px');
        }
    }
}
