import { Component, OnInit, Input, ElementRef, EventEmitter, Output, ViewChild } from '@angular/core';

import { TooltipDirective } from './../tooltip.directive';

@Component({
    selector: 'as4-clipboard, [as4-clipboard]',
    template: `
       <i class="fa fa-clipboard clickable" ngxClipboard [cbContent]="content" (cbOnSuccess)="success()" [as4-tooltip]="message" as4-tooltip-manual="true" #tooltip="as4-tooltip"></i>       
   `
})
export class ClipboardComponent {
    @Input() public message: string = 'Copied to clipboard';
    @Input() public content: string;
    @Output() public onCopied: EventEmitter<boolean> = new EventEmitter();
    @ViewChild('tooltip') public tooltip: TooltipDirective;
    public success() {
        this.onCopied.emit(true);
        this.tooltip.show();
    }
}
