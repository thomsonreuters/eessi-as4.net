import { Component, OnInit, Input } from '@angular/core';

import { ItemType } from './../api/ItemType';
import { Settings } from './../api/Settings';

@Component({
    selector: 'as4-Settings',
    templateUrl: './Settings.component.html'
})
export class SettingsComponent implements OnInit {
    @Input() properties: ItemType[];
    constructor() {
    }

    ngOnInit() {
    }
}
