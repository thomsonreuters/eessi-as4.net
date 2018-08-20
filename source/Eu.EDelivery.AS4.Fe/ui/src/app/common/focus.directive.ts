import { Directive, DoCheck, ElementRef, AfterViewInit, Input, ViewChildren, QueryList, ContentChildren, SkipSelf, Optional } from '@angular/core';

@Directive({
  selector: '[tabIndex]'
})
export class TabIndexDirective {
  @Input() public tabIndex: number;
  constructor(private el: ElementRef) { }
  public focus() {
    this.el.nativeElement.focus();
  }
}

// tslint:disable-next-line:max-classes-per-file
@Directive({
  selector: '[focus]'
})
export class FocusDirective implements DoCheck {
  @Input() public onlyWhenNoText: boolean = false;
  // tslint:disable-next-line:no-input-rename
  @Input('focus-disabled') public disabled: boolean = false;
  @ContentChildren(FocusDirective) public tabIndexes: QueryList<FocusDirective>;
  private initialised: boolean = false;
  constructor(private _el: ElementRef, @SkipSelf() @Optional() private _focusDirective: FocusDirective) { }
  public ngDoCheck() {
    if (!!this._focusDirective || this.disabled) {
      // Do nothing because the parent focus directive will take over the logic.
      return;
    }
    if (this.initialised) {
      return;
    }
    setTimeout(() => {
      if (!!this.tabIndexes && this.tabIndexes.length > 0) {
        const first = this.tabIndexes.first;
        first.focus();
        return;
      }

      if (this.onlyWhenNoText && (<any>document.activeElement).type === 'text') {
        return;
      }
      this._el.nativeElement.focus();
    });
    this.initialised = true;
  }
  public focus() {
    this._el.nativeElement.focus();
  }
}
