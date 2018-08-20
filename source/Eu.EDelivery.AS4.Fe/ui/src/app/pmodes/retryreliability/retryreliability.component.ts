import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
  selector: 'as4-retryreliability',
  template: `
  <div [formGroup]="group">
    <div class="sub-header-1">Retry Reliability</div>
      <as4-input label="Enabled" runtimeTooltip="reliability.isenabled">
        <input type="checkbox" data-cy="retry.isEnabled" formControlName="isEnabled">
      </as4-input>
      <as4-input label="Retry count" runtimeTooltip="reliability.retrycount">
        <input type="number" data-cy="retry.count" formControlName="retryCount" 
           />
      </as4-input>
      <as4-input label="Retry interval" runtimeTooltip="reliability.retryinterval">
        <input type="text" [textMask]="{ mask: mask }" data-cy="retry.interval" formControlName="retryInterval" 
         />
      </as4-input>
  </div>`
})
export class RetryReliabilityComponent {
  @Input() public group: FormGroup;
  public mask: any[] = [
    /[0-9]/,
    ':',
    /[0-9]/,
    /[0-9]/,
    ':',
    /[0-5]/,
    /[0-9]/,
    ':',
    /[0-5]/,
    /[0-9]/
  ];
}
