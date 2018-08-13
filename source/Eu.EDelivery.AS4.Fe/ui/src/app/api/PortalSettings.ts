export class PortalSettings {
    public port: string;
    public settings: {
        showStacktraceInException: boolean,
        settingsXml: string,
        runtime: string
    };
    public authentication: {
        connectionString: string,
        provider: string
    };
    public monitor: {
        provider: string,
        connectionString: string
    };
    public pmodes: {
        sendingPmodeFolder: string,
        receivingPmodeFolder: string
    };
    public submitTool: {
        toHttpAddress: string,
        payloadHttpAddress: string
    };
}
