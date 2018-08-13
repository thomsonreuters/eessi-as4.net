import { ChangeDetectionStrategy, Component, Input, ChangeDetectorRef } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
    selector: 'as4-file-select',
    templateUrl: 'fileselect.component.html',
    styleUrls: ['./fileselect.component.scss'],
    providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: FileSelectComponent, multi: true }],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FileSelectComponent implements ControlValueAccessor {
    @Input() public fileNameControl: FormControl;
    public value: any;
    private onChange: (_) => void;
    constructor(private changeDetector: ChangeDetectorRef) {}
    public writeValue(obj: any): void {
        this.value = obj;
    }
    public registerOnChange(fn: any): void {
        this.onChange = fn;
    }
    public registerOnTouched(): void {}
    public setDisabledState?(isDisabled: boolean): {};
    public fileSelected(event) {
        let reader = new FileReader();
        const fileName = event.currentTarget.files[0].name;
        reader.onload = () => {
            if (!!this.fileNameControl) {
                this.fileNameControl.parent.markAsDirty();
                this.fileNameControl.parent.markAsTouched();
                this.fileNameControl.setValue(fileName);
            }
            this.onChange(reader.result);
            this.changeDetector.markForCheck();
        };
        reader.readAsDataURL(event.currentTarget.files[0]);
    }
}
