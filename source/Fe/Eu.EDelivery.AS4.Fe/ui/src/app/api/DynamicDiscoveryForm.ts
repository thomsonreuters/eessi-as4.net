/* tslint:disable */
import { FormWrapper } from './../common/form.service';
import { DynamicDiscovery } from './DynamicDiscovery';

export class DynamicDiscoveryForm {
    public static getForm(formBuilder: FormWrapper, current: DynamicDiscovery | null): FormWrapper {
        return formBuilder
            .group({
                [DynamicDiscovery.FIELD_smlScheme]: [!!!current ? this.defaultSmlScheme : (current && current.smlScheme)],
                [DynamicDiscovery.FIELD_smpServerDomainName]: [current && current.smpServerDomainName],
                [DynamicDiscovery.FIELD_documentIdentifier]: [!!!current ? this.defaultDocumentIdentifier : current && current.documentIdentifier],
                [DynamicDiscovery.FIELD_documentIdentifierScheme]: [!!!current ? this.defaultDocumentIdentifierString : current && current.documentIdentifierScheme]
            });
    }
    private static defaultSmlScheme = 'iso6523-actorid-upis';
    private static defaultDocumentIdentifier = 'urn:oasis:names:specification:ubl:schema:xsd:Invoice-2::Invoice##urn:www.cenbii.eu:transaction:biitrns010:ver2.0:extended:urn:www.peppol.eu:bis:peppol5a:ver2.0::2.1';
    private static defaultDocumentIdentifierString = 'busdox-docid-qns';
}
