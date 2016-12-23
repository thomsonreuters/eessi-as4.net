import { Component, OnInit, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
    selector: '[crud-buttons]',
    template: `
            <button type="button" class="btn btn-flat rename-button" (click)="rename.emit()" [disabled]="!!!current"><i class="fa fa-edit"></i></button>
            <button type="button" class="btn btn-flat add-button" (click)="add.emit()" [disabled]="isNewMode"><i class="fa fa-plus"></i></button>
            <button type="button" class="btn btn-flat save-button" (click)="save.emit()" [class.btn-primary]="form.dirty || isNewMode" [disabled]="!form.dirty && !isNewMode"><i class="fa fa-save"></i></button>
            <button type="button" class="btn btn-flat delete-button" (click)="delete.emit()" [disabled]="!!!current"><i class="fa fa-trash-o"></i></button>
            <button type="button" class="btn btn-flat reset-button" (click)="reset.emit()" [class.btn-primary]="form.dirty || isNewMode" [disabled]="!form.dirty && !isNewMode"><i class="fa fa-undo"></i></button>
            <ng-content></ng-content>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CrudButtonsComponent implements OnInit {
    @Output() rename = new EventEmitter();
    @Output() add = new EventEmitter();
    @Output() save = new EventEmitter();
    @Output() delete = new EventEmitter();
    @Output() reset = new EventEmitter();
    @Input() current: string;
    @Input() form: FormGroup;
    @Input() isNewMode: boolean = false;
    constructor() {
    }

    ngOnInit() {
    }
}
