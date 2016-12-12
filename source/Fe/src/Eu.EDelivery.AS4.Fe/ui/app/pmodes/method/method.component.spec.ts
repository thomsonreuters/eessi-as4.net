import { ModalService } from './../../common/modal.service';
import { Property } from './../../api/Property';
import { RuntimeStore } from './../../settings/runtime.store';
import { Method } from './../../api/Method';
import { ReactiveFormsModule, FormArray, FormBuilder } from '@angular/forms';
import { By } from '@angular/platform-browser/src/dom/debug/by';
import { MethodComponent } from './method.component';
import { Observable } from 'rxjs';
import {
    inject,
    TestBed,
    ComponentFixture,
    async
} from '@angular/core/testing';
import { Component } from '@angular/core';
import {
    BaseRequestOptions,
    ConnectionBackend,
    Http
} from '@angular/http';
import { MockBackend } from '@angular/http/testing';

import { As4ComponentsModule } from './../../common/as4components.module';
import { CommonModule } from '@angular/common';
import { DialogService } from './../../common/dialog.service';
import { InputComponent } from '../../common/input/input.component';
import { ItemType } from './../../api/ItemType';

describe('Notify method', () => {
    let fixture: ComponentFixture<MethodComponent>;
    let instance: MethodComponent;
    let types: Array<ItemType>;
    let itemType1: ItemType;
    let property1: Property;
    let method: Method;
    beforeEach(async(() => TestBed.configureTestingModule({
        declarations: [
            InputComponent,
            MethodComponent
        ],
        imports: [
            ReactiveFormsModule,
        ],
        providers: [
            DialogService,
            RuntimeStore,
            FormBuilder,
            ModalService
        ]
    }).compileComponents()));
    beforeEach(() => {
        fixture = TestBed.createComponent(MethodComponent);
        instance = fixture.componentInstance;

        itemType1 = new ItemType();
        itemType1.name = 'name';
        itemType1.technicalName = 'technicalName';
        itemType1.properties = new Array<Property>();
        types = new Array<ItemType>();
        types.push(itemType1);
        property1 = new Property();
        property1.description = 'description';
        property1.friendlyName = 'friendlyName';
        itemType1.properties.push(property1);

        method = new Method();
    });
    afterEach(() => fixture.destroy());
    describe('typechanged', () => {
        it('should set the parameters form value', inject([FormBuilder], (formBuilder: FormBuilder) => {
            instance.group = Method.getForm(formBuilder, method);
            instance.types = types;
            instance.typeChanged(itemType1.name);

            let form = <Method>instance.group.value;
            expect(form.parameters.length).toBe(1);
            let param = form.parameters[0];
            expect(param.name).toBe(property1.friendlyName);
        }));
    });
});
