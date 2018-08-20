import { Component, Input, ChangeDetectionStrategy, EventEmitter, Output, OnChanges } from '@angular/core';

@Component({
    selector: 'as4-pager, [as4-pager]',
    template: `
        <div class="row">
            <div *ngIf="!!pageTotal && !!total" class="col-xs-4 summary text-right">{{pageTotal}} of {{total}}</div>
            <div *ngIf="!!pageTotal && !!total" class="col-xs-6 text-right pull-right">
                <ul *ngIf="pager.pages && pager.pages.length" class="pagination">
                    <li [ngClass]="{disabled:pager.currentPage === 1}">
                        <a (click)="!(pager.currentPage === 1) && onChangePage.emit(1)" class="hover">First</a>
                    </li>
                    <li [ngClass]="{disabled:pager.currentPage === 1}">
                        <a (click)="!(pager.currentPage === 1) && onChangePage.emit(pager.currentPage-1)" class="hover">Previous</a>
                    </li>
                    <li *ngFor="let page of pager.pages" [ngClass]="{active:pager.currentPage === page}">
                        <a (click)="!(pager.currentPage === page) && onChangePage.emit(page)" class="hover">{{page}}</a>
                    </li>
                    <li [ngClass]="{disabled:pager.currentPage === pager.totalPages}">
                        <a (click)="!(pager.currentPage === pager.totalPages) && onChangePage.emit(pager.currentPage + 1)" class="hover">Next</a>
                    </li>
                    <li [ngClass]="{disabled:pager.currentPage === pager.totalPages}">
                        <a (click)="!(pager.currentPage === pager.totalPages) && onChangePage.emit(pager.totalPages)" class="hover">Last</a>
                    </li>
                </ul>
            </div>
        </div>
        `,
    styles: [`
        .pagination {
            margin: 0;
        }   
        .summary {
            padding-top: 5px;
        }
        .hover {
            cursor: pointer
        }
    `],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PagerComponent implements OnChanges {
    @Input() public pages: number;
    @Input() public page: number;
    @Input() public total: number;
    @Input() public pageTotal: number;
    @Output() public onChangePage: EventEmitter<number> = new EventEmitter();
    public pager: any;
    public ngOnChanges() {
        this.pager = this.getPager(this.pages, this.page, 10);
    }
    public getPager(totalItems: number, currentPage: number = 1, pageSize: number = 10) {
        // calculate total pages
        let totalPages = Math.ceil(totalItems / pageSize);
        let startPage: number;
        let endPage: number;
        if (totalPages <= 10) {
            // less than 10 total pages so show all
            startPage = 1;
            endPage = totalItems;
            totalPages = totalItems;
        } else {
            // more than 10 total pages so calculate start and end pages
            if (currentPage <= 6) {
                startPage = 1;
                endPage = 10;
            } else if (currentPage + 4 >= totalPages) {
                startPage = totalPages - 9;
                endPage = totalPages;
            } else {
                startPage = currentPage - 5;
                endPage = currentPage + 4;
            }
        }
        // calculate start and end item indexes
        let startIndex = (currentPage - 1) * pageSize;
        let endIndex = Math.min(startIndex + pageSize - 1, totalItems - 1);
        // create an array of pages to ng-repeat in the pager control
        let pages = new Array<number>();
        for (let page = startPage; page < (endPage + 1); page++) {
            pages.push(page);
        }
        // return object with all pager properties required by the view
        return {
            totalItems,
            currentPage,
            pageSize,
            totalPages,
            startPage,
            endPage,
            startIndex,
            endIndex,
            pages
        };
    }
}
