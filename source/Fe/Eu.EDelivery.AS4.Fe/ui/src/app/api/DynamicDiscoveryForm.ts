import { FormGroup, AbstractControl, Validators } from '@angular/forms';

/* tslint:disable */
import { SendingPmode } from './SendingPmode';
import { FormWrapper } from './../common/form.service';
import { DynamicDiscovery } from './DynamicDiscovery';

export class DynamicDiscoveryForm {
    public static getForm(formBuilder: FormWrapper, current: DynamicDiscovery | null): FormWrapper {
        let isNew = !!!current;
        return formBuilder
            .group({
                [DynamicDiscovery.FIELD_smlScheme]: [!!!current ? this.defaultSmlScheme : (current && current.smlScheme), Validators.required],
                [DynamicDiscovery.FIELD_smpServerDomainName]: [current && current.smpServerDomainName, Validators.required],
                [DynamicDiscovery.FIELD_documentIdentifier]: [!!!current ? this.defaultDocumentIdentifier : current && current.documentIdentifier, Validators.required],
                [DynamicDiscovery.FIELD_documentIdentifierScheme]: [!!!current ? this.defaultDocumentIdentifierString : current && current.documentIdentifierScheme, Validators.required]
            })
            .onStatusChange(DynamicDiscovery.FIELD_smlScheme, (status, wrapper) => {
                if (isNew) {
                    isNew = false;

                    const sml = wrapper.form.get(DynamicDiscovery.FIELD_smlScheme);
                    const documentIdent = wrapper.form.get(DynamicDiscovery.FIELD_documentIdentifier);
                    const documentIdentScheme = wrapper.form.get(DynamicDiscovery.FIELD_documentIdentifierScheme);

                    sml!.setValue(this.defaultSmlScheme);
                    documentIdent!.setValue(this.defaultDocumentIdentifier);
                    documentIdentScheme!.setValue(this.defaultDocumentIdentifierString);
                }
            });
    }
    private static defaultSmlScheme = 'iso6523-actorid-upis';
    private static defaultDocumentIdentifier = 'urn:oasis:names:specification:ubl:schema:xsd:Invoice-2::Invoice##urn:www.cenbii.eu:transaction:biitrns010:ver2.0:extended:urn:www.peppol.eu:bis:peppol5a:ver2.0::2.1';
    private static defaultDocumentIdentifierString = 'busdox-docid-qns';
}
