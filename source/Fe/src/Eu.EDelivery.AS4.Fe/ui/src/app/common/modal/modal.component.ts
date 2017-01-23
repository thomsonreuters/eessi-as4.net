import { Subject } from 'rxjs/Subject';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observer } from 'rxjs/Observer';
import { Observable } from 'rxjs';
import { Component, Input, Output, OnDestroy, EventEmitter, HostListener, ViewChild, OnInit, ElementRef } from '@angular/core';

import { ModalService } from './modal.service';

@Component({
    selector: 'as4-modal',
    template: `
        <div *ngIf="isVisible" class="modal fade" [class.in]="isVisible" [class.show-modal]="isVisible" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" #modal>
            <div class="modal-dialog" role="document" [ngClass]="{ 'modal-danger': !!type }">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" (click)="cancel()" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                        </button>
                        <h4 class="modal-title" id="myModalLabel">{{title}}</h4>
                    </div>
                    <div class="modal-body" *ngIf="isVisible">
                        <b *ngIf="showDefaultMessage">{{message}}</b>
                        <ng-content></ng-content>
                    </div>
                    <div class="modal-footer">                    
                        <div *ngIf="showDefaultButtons === true">
                            <button type="button" class="btn btn-flat" *ngIf="showCancel" data-dismiss="modal" (click)="cancel()" focus onlyWhenNoText="true">{{buttonCancel}}</button>
                            <button type="button" class="btn btn-flat" *ngIf="showOk" (click)="ok()">{{buttonOk}}</button>
                        </div>
                        <ng-content select="[buttons]"></ng-content>
                    </div>
                </div>
            </div>
        </div>
   `,
    styles: ['./modal.component.scss']
})
export class ModalComponent implements OnDestroy {
    public isVisible: boolean = false;
    public result: any | null;
    public type: string = '';
    public showOk: boolean = true;
    public showCancel: boolean = true;
    @Input() public payload: string;
    @Input() public showDefaultButtons: boolean = true;
    @Input() public name: string;
    @Input() public title: string;
    @Input() public message: string;
    @Input() public showDefaultMessage: boolean = true;
    @Input() public buttonOk: string = 'Ok';
    @Input() public buttonCancel: string = 'Cancel';
    @Output() public shown = new EventEmitter();
    @HostListener('document:keydown', ['$event'])
    private obs: Subject<boolean>;
    constructor(private modalService: ModalService, private elementRef: ElementRef) {
        this.modalService.registerModal(this);
    }
    public keyDown(event: KeyboardEvent) {
        if (!this.isVisible) {
            return;
        }
        if (event.keyCode === 27) {
            event.stopPropagation();
            event.preventDefault();
        }
        if (event.keyCode === 27) {
            this.cancel();
        }
    }
    public cancel() {
        this.isVisible = false;
        this.obs.next(false);
        this.obs.complete();
    }
    public ok() {
        this.isVisible = false;
        this.obs.next(true);
        this.obs.complete();
    }
    public show(): Observable<boolean> {
        this.reset();
        // Wrap the shown emit event in a timeout to make sure that the modal dialog has already been rendered
        setTimeout(() => this.shown.emit());
        this.obs = new Subject<boolean>();
        return this.obs.asObservable();
    }
    public ngOnDestroy() {
        this.modalService.unregisterModal(this);
        console.log('destroyed');
    }
    private reset() {
        this.result = null;
        this.isVisible = true;
    }
}
