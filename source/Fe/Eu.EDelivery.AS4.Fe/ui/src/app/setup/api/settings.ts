export class Settings {
    public adminPassword: string;
    public readonlyPassword: string;
    public jwtKey: string;
    constructor(init?: Partial<Settings>) {
        Object.assign(this, init);
    }
}
