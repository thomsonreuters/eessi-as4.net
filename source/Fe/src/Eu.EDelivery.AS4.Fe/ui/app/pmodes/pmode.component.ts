import { FormGroup, FormBuilder } from '@angular/forms';
import { Component, OnInit } from '@angular/core';

import { PmodesModule } from './pmodes.module';

@Component({
    selector: 'as4-pmode',
    template: `
        <form [formGroup]="form" class="form-horizontal">
            <as4-box>
                <div content class="col-md-6 col-xs-12">
                    <as4-input label="Id">
                        <input type="text" class="form-control" formControlName="id"/>
                    </as4-input>
                    <as4-input label="Message exchange pattern">
                        <select class="form-control">
                            <option>OneWay</option>
                            <option>TwoWay</option>
                        </select>
                    </as4-input>
                    <as4-input label="Message exchange pattern binding">
                        <select class="form-control">
                            <option>Pull</option>
                            <option>Push</option>
                        </select>
                    </as4-input>
                </div>
            </as4-box>
        </form>
    `
})
export class PmodeComponent implements OnInit {
    public form: FormGroup;
    constructor(private formBuilder: FormBuilder) {
        this.form = formBuilder.group({
            id: [''],
            mep: ['']
        });
    }

    ngOnInit() {
    }
}
