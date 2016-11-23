import { Component, OnInit, Input, ElementRef, Renderer } from '@angular/core';

@Component({
    selector: 'as4-box',
    templateUrl: './box.component.html',
    styles: [
        `
            :host {
                display: flex;
                flex-direction: column;
                width: calc(50vw - 160px);         
                margin-right: 10px;
                background-color: white;
                margin-bottom: 10px;
                border-radius: 3px;           
            }     

            @media (max-width: 960px) {
                :host {
                    width: 100vh;
                }
            }       

            .box-footer {
                border-top: 0 !important;
            }

            .box-primary {
                border-top: 2px solid green;
            }
        `
    ]
})
export class BoxComponent implements OnInit {
    @Input() title: string;
    @Input() collapsed: boolean = true;
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