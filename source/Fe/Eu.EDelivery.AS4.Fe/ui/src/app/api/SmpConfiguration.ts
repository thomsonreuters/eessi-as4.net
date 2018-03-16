export class SmpConfiguration {
    id: number;
    toPartyId: string;
    partyRole: string;
    partyType: string;
    url: string;
    serviceValue: string;
    serviceType: string;
    action: string;
    tlsEnabled: boolean;
    encryptionEnabled: boolean;
    finalRecipient: string;
    encryptAlgorithm: string;
    encryptAlgorithmKeySize: number;
    encryptPublicKeyCertificate: string;
    encryptKeyDigestAlgorithm: string;
    encryptKeyMgfAlorithm: string;
    encryptKeyTransportAlgorithm: string;

    static FIELD_id = 'id';
    static FIELD_ToPartyId = 'toPartyId';
    static FIELD_PartyRole = 'partyRole';
    static FIELD_PartyType = 'partyType';
    static FIELD_Url = 'url';
    static FIELD_ServiceValue = 'serviceValue';
    static FIELD_ServiceType = 'serviceType';
    static FIELD_Action = 'action';
    static FIELD_TlsEnabled = 'tlsEnabled';
    static FIELD_EncryptionEnabled = 'encryptionEnabled';
    static FIELD_FinalRecipient = 'finalRecipient';
    static FIELD_EncryptAlgorithm = 'encryptAlgorithm';
    static FIELD_EncryptAlgorithmKeySize = 'encryptAlgorithmKeySize';
    static FIELD_EncryptPublicKeyCertificate = 'encryptPublicKeyCertificate';
    static FIELD_EncryptKeyDigestAlgorithm = 'encryptKeyDigestAlgorithm';
    static FIELD_EncryptKeyMgfAlorithm = 'encryptKeyMgfAlorithm';
    static FIELD_EncryptKeyTransportAlgorithm = 'encryptKeyTransportAlgorithm';
}