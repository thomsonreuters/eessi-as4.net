import { Component, OnInit, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
    selector: '[crud-buttons]',
    template: `
        <button *ngIf="showRename" as4-tooltip="Rename" type="button" as4-auth class="btn btn-flat rename-button" (click)="rename.emit()" [attr.disabled]="!!!current || null"><i class="fa fa-edit"></i></button>
        <button as4-tooltip="New" type="button" as4-auth class="btn btn-flat add-button" (click)="add.emit()" [attr.disabled]="isNewMode || null"><i class="fa fa-plus"></i></button>
        <button as4-tooltip="Save changes" type="button" as4-auth class="btn btn-flat save-button" (click)="save.emit()" [class.btn-primary]="form.dirty || isNewMode" [attr.disabled]="(!form.dirty && !isNewMode) || null"><i class="fa fa-save"></i></button>
        <button as4-tooltip="Delete" type="button" as4-auth class="btn btn-flat delete-button" (click)="delete.emit()" [attr.disabled]="!!!current || null"><i class="fa fa-trash-o"></i></button>
        <button as4-tooltip="Undo all changes" type="button" as4-auth class="btn btn-flat reset-button" (click)="reset.emit()" [class.btn-primary]="form.dirty || isNewMode" [attr.disabled]="(!form.dirty && !isNewMode) || null"><i class="fa fa-undo"></i></button>
        <ng-content></ng-content>
    `
})
export class CrudButtonsComponent {
    @Output() public rename = new EventEmitter();
    @Output() public add = new EventEmitter();
    @Output() public save = new EventEmitter();
    @Output() public delete = new EventEmitter();
    @Output() public reset = new EventEmitter();
    @Input() public current: string;
    @Input() public form: FormGroup;
    @Input() public isNewMode: boolean = false;
    @Input() public showRename: boolean = true;
}
