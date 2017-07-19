import { Component, OnInit, Input, ElementRef, EventEmitter, Output, ViewChild } from '@angular/core';

import { TooltipDirective } from './../tooltip.directive';

@Component({
    selector: 'as4-clipboard, [as4-clipboard]',
    template: `
        <div class="clipboard-container" as4-tooltip="Copy to clipboard" #outerTooltip="as4-tooltip">
            <i class="fa fa-clipboard clickable" ngxClipboard [cbContent]="content" (cbOnSuccess)="success()" [as4-tooltip]="message" as4-tooltip-manual="true" #tooltip="as4-tooltip"></i>       
        </div>
   `,
   styles: [`
        .clipboard-container {
            display: inline;
        }
        .fa-clipboard {
            color: #CCC;
        }
   `]
})
export class ClipboardComponent {
    @Input() public message: string = 'Copied to clipboard';
    @Input() public content: string;
    @Output() public onCopied: EventEmitter<boolean> = new EventEmitter();
    @ViewChild('tooltip') public tooltip: TooltipDirective;
    @ViewChild('outerTooltip') public outerToolTip: TooltipDirective;
    public success() {
        this.outerToolTip.hide();
        this.onCopied.emit(true);
        setTimeout(() => this.tooltip.show());
    }
}
