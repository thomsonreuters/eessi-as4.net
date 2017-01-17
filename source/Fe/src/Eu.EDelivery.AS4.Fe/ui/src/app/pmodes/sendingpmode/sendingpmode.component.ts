import { PMODECRUD_SERVICE } from './../crud/crud.component';
import { SendingPmodeService } from './../pmode.service';
import { SendingProcessingMode } from './../../api/SendingProcessingMode';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs/Observable';

import { BasePmodeComponent } from './../basepmode/basepmode.component';
import { SendingPmode } from './../../api/SendingPmode';
import { ReceivingPmode } from './../../api/ReceivingPmode';

@Component({
    selector: 'as4-sending-pmode',
    templateUrl: './sendingpmode.component.html',
    styles: ['../basepmode/basepmode.component.scss'],
    providers: [
        { provide: PMODECRUD_SERVICE, useClass: SendingPmodeService }
    ]
})
export class SendingPmodeComponent {
    public mask: Array<any> = [/[0-6]/, /[0-6]/, ':', /[0-6]/, /[0-6]/, ':', /[0-6]/, /[0-6]/];
}
