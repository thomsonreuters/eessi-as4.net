/* tslint: disable */
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

export class KeyEncryption {
    transportAlgorithm: string;
    digestAlgorithm: string;
    mgfAlgorithm: string;

    static defaultTransportAlgorithm: string = 'http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p';
    static defaultDigestAlgorithm: string = 'http://www.w3.org/2000/09/xmldsig#sha1';
    static transportAlgorithm: string = 'http://www.w3.org/2009/xmlenc11#rsa-oaep';

    static FIELD_transportAlgorithm: string = 'transportAlgorithm';
    static FIELD_digestAlgorithm: string = 'digestAlgorithm';
    static FIELD_mgfAlgorithm: string = 'mgfAlgorithm';

    static defaultMgfAlgorithm: string = 'http://www.w3.org/2009/xmlenc11#mgf1sha1';

    static getForm(formBuilder: FormBuilder, current: KeyEncryption): FormGroup {
        let form = formBuilder.group({
            [this.FIELD_transportAlgorithm]: [current && current.transportAlgorithm, Validators.required],
            [this.FIELD_digestAlgorithm]: [current && current.digestAlgorithm, Validators.required],
            [this.FIELD_mgfAlgorithm]: [current && current.mgfAlgorithm, Validators.required]
        });
        this.setupForm(form, current);
        return form;
    }
    static patchForm(formBuilder: FormBuilder, form: FormGroup, current: KeyEncryption, isDisabled: boolean) {
        form.get(this.FIELD_transportAlgorithm).reset({ value: (current && current.transportAlgorithm) || this.defaultTransportAlgorithm, disabled: !!!current || isDisabled });
        form.get(this.FIELD_digestAlgorithm).reset({ value: (current && current.digestAlgorithm) || this.defaultDigestAlgorithm, disabled: !!!current || isDisabled });
        form.get(this.FIELD_mgfAlgorithm).reset({ value: (current && current.mgfAlgorithm) || this.defaultMgfAlgorithm, disabled: !!!current || isDisabled || current.transportAlgorithm !== this.transportAlgorithm });
    }
    static setupForm(form: FormGroup, current: KeyEncryption) {
        form.get(this.FIELD_transportAlgorithm)
            .valueChanges
            .subscribe((result) => this.updateMgfAlgorithmState(form, result));
    }
    static updateMgfAlgorithmState(form: FormGroup, state: string): boolean {
        let mgf = form.get(this.FIELD_mgfAlgorithm);
        if (state !== this.transportAlgorithm) {
            setTimeout(() => mgf.disable());
            return false;
        } else {
            setTimeout(() => mgf.enable());
            return true;
        }
    }
}
