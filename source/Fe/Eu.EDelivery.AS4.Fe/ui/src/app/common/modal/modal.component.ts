import { Subject } from 'rxjs/Subject';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observer } from 'rxjs/Observer';
import { Observable } from 'rxjs/Observable';
// tslint:disable-next-line:max-line-length
import { Component, Input, Output, OnDestroy, EventEmitter, HostListener, ViewChild, OnInit, ElementRef, ViewEncapsulation, Renderer } from '@angular/core';

import { ModalService } from './modal.service';

@Component({
    selector: 'as4-modal, [as4-modal]',
    template: `
        <div *ngIf="isVisible" class="modal fade" [class.in]="isVisible" [class.show-modal]="isVisible" id="myModal" [ngClass]="{ 'zIndexTop': type === 'modal-danger' || !!!type, 'zIndexNormal': type !== 'modal-danger'}" role="dialog" aria-labelledby="myModalLabel" #modal>
            <div class="modal-dialog" role="document" [ngClass]="{ 'zIndexTop': type === 'modal-danger' || !!!type, 'zIndexNormal': type !== 'modal-danger', 'modal-danger': !!type }" focus>
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" *ngIf="showClose" class="close" (click)="cancel()" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                        </button>
                        <h4 class="modal-title" id="myModalLabel">{{title}}</h4>
                    </div>
                    <div class="modal-body" #body>
                        <div *ngIf="showDefaultMessage">{{message}}</div>
                        <ng-content></ng-content>
                    </div>
                    <div class="modal-footer">
                        <div *ngIf="showDefaultButtons === true">
                            <button type="button" class="btn btn-flat" *ngIf="showOk" (click)="ok()" focus>{{buttonOk}}</button>
                            <button type="button" class="btn btn-flat" *ngIf="showCancel" data-dismiss="modal" (click)="cancel()" focus onlyWhenNoText="true">{{buttonCancel}}</button>
                        </div>
                        <ng-content select="[buttons]"></ng-content>
                    </div>
                </div>
            </div>
        </div>
   `,
    styleUrls: ['modal.component.scss'],
    encapsulation: ViewEncapsulation.None,
    exportAs: 'as4-modal'
})
export class ModalComponent implements OnDestroy {
    public isVisible: boolean = false;
    public result: any | null;
    public type: string = '';
    public showOk: boolean = true;
    public showCancel: boolean = true;
    public get transition(): string {
        return this.isVisible ? 'enter' : 'exit';
    }
    public callBack: () => void;
    @Input() public payload: any;
    @Input() public showDefaultButtons: boolean = true;
    @Input() public name: string;
    @Input() public title: string | undefined;
    @Input() public message: string;
    @Input() public showDefaultMessage: boolean = true;
    @Input() public buttonOk: string = 'OK';
    @Input() public buttonCancel: string = 'CANCEL';
    @Input() public okAction: () => void | null;
    @Input() public noReset: boolean = false;
    @Input() public showClose: boolean = true;
    @Input() public canClose: boolean = true;
    @Output() public shown = new EventEmitter();
    @ViewChild('body') public bodyEl: ElementRef;
    private obs: Subject<boolean>;
    constructor(private modalService: ModalService, private elementRef: ElementRef, private _renderer: Renderer) {
        this.modalService.registerModal(this);
    }
    @HostListener('document:keydown', ['$event'])
    public keyDown(event: KeyboardEvent): void {
        if (!this.isVisible || !this.canClose) {
            return;
        }
        if (event.keyCode === 27) {
            event.stopPropagation();
            event.preventDefault();
            this.cancel();
        }
    }
    public cancel(): void {
        this.isVisible = false;
        this.obs.next(false);
        this.obs.complete();
    }
    public ok(): void {
        if (!!this.okAction) {
            this.okAction();
        }
        this.isVisible = false;
        this.obs.next(true);
        this.obs.complete();
    }
    public show(): Observable<boolean> {
        this.obs = new Subject<boolean>();
        if (!this.noReset) {
            this.reset();
        }
        this.isVisible = true;
        // wrap the shown emit event in a timeout to make sure that the modal dialog has already been rendered
        setTimeout(() => this.shown.emit());
        return this.obs.asObservable();
    }
    public ngOnDestroy(): void {
        this.modalService.unregisterModal(this);
    }
    private reset(): void {
        this.result = null;
    }
}
