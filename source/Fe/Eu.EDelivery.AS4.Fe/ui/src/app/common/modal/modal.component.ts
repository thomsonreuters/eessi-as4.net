import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnDestroy,
  Output,
  Renderer,
  ViewChild,
  ViewEncapsulation,
} from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';

import { ModalService } from './modal.service';

// tslint:disable-next-line:max-line-length
@Component({
  selector: 'as4-modal, [as4-modal]',
  template: `
        <div *ngIf="isVisible" class="modal fade" (keyup.enter)="enter($event)" [class.in]="isVisible" [class.show-modal]="isVisible" id="myModal" [ngClass]="{ 'zIndexTop': type === 'modal-danger' || !!!type, 'zIndexNormal': type !== 'modal-danger'}" role="dialog" aria-labelledby="myModalLabel" #modal>
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
                    <div class="modal-footer" *ngIf="!unexpected">
                        <div *ngIf="showDefaultButtons === true">
                            <button type="button" class="btn btn-flat" data-cy="ok" *ngIf="showOk" (click)="ok()" focus>{{buttonOk}}</button>
                            <button type="button" class="btn btn-flat" data-cy="cancel" *ngIf="showCancel" data-dismiss="modal" (click)="cancel()" focus onlyWhenNoText="true">{{buttonCancel}}</button>
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
  public enter(event: KeyboardEvent & { target: { value: string } }) {
    if (event.keyCode === 13) {
      this.ok();
    }
  }
  @Input() public isVisible: boolean = false;
  public result: any | null;
  public type: string = '';
  public showOk: boolean = true;
  public showCancel: boolean = true;
  public unexpected: boolean = false;
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
  @Output() public okEvent = new EventEmitter<boolean>();
  @ViewChild('body') public bodyEl: ElementRef;
  private obs: Subject<boolean>;
  constructor(
    private modalService: ModalService,
    private elementRef: ElementRef,
    private _renderer: Renderer
  ) {
    this.modalService.registerModal(this);
    this.obs = new Subject<boolean>();
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
    this.okEvent.emit(false);
    this.isVisible = false;
    this.obs.next(false);
    this.obs.complete();
  }
  public ok(): void {
    this.okEvent.emit(true);
    if (!!this.okAction) {
      this.okAction();
    }
    this.isVisible = false;
    this.obs.next(true);
    this.obs.complete();
  }
  public show(): Observable<boolean> {
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
