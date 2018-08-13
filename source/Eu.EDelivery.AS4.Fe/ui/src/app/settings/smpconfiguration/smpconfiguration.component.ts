import 'rxjs/add/operator/debounceTime';

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Subject } from 'rxjs';

import { SmpConfigurationRecord } from '../../api/SmpConfiguration';
import { DialogService } from '../../common/dialog.service';
import { ModalService } from '../../common/modal/modal.service';
import { SmpConfigurationService } from './smpconfiguration.service';
import { SmpConfigurationDetailComponent } from './smpconfigurationdetail.component';

@Component({
  selector: 'as4-smpconfiguration',
  templateUrl: './smpconfiguration.component.html',
  styleUrls: ['./smpconfiguration.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SmpConfigurationComponent {
  public configs: Subject<SmpConfigurationRecord[]>;
  constructor(
    private smpService: SmpConfigurationService,
    private modalService: ModalService,
    private dialogService: DialogService
  ) {
    this.configs = new Subject<SmpConfigurationRecord[]>();
    this.smpService.get().subscribe((results) => {
      this.configs.next(results);
    });
  }

  public addItem() {
    this.modalService
      .showComponent(SmpConfigurationDetailComponent, (cmp, factory) => {
        cmp.setRecordId(0);
        cmp.componentRef = factory;
      })
      .subscribe(() => {
        this.smpService.get().subscribe((results) => {
          this.configs.next(results);
        });
      });
  }

  public editItem(id: number) {
    this.modalService
      .showComponent(SmpConfigurationDetailComponent, (cmp, factory) => {
        cmp.setRecordId(id);
        cmp.componentRef = factory;
      })
      .subscribe(() => {
        this.smpService.get().subscribe((results) => {
          this.configs.next(results);
        });
      });
  }
  public deleteItem(id: number) {
    this.dialogService
      .confirm('Are you sure you want to delete the configuration?')
      .subscribe((result) => {
        if (!!result) {
          this.smpService.delete(id).subscribe();
          this.smpService.get().subscribe((results) => {
            this.configs.next(results);
          });
        }
      });
  }
}
