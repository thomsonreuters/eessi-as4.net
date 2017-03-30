export class MessageResult<T> {
    public messages: T[];
    public total: number;
    public currentPage: number;
    public pages: number;
    public page: number;
}
