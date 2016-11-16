import { Component, OnInit, Input } from '@angular/core';

@Component({
    selector: 'as4-box',
    templateUrl: './box.component.html'
})
export class BoxComponent implements OnInit {
    @Input() title: string;
    @Input() collapsed: boolean = true;
    constructor() {
    }

    ngOnInit() {
    }
}