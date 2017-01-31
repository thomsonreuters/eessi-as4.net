import { SortOrder } from './sortorder.enum';

export interface ISortDictionary {
    [field: string]: SortOrder;
}
