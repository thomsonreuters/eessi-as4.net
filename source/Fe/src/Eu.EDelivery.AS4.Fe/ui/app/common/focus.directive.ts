import { Directive, DoCheck, ElementRef, AfterViewInit, Input } from '@angular/core';

@Directive({
  selector: '[focus]'
})
export class FocusDirective implements DoCheck {
  @Input() onlyWhenNoText: boolean = false;
  private initialised: boolean = false;
  constructor(private el: ElementRef) { }
  ngDoCheck() {
    if (this.initialised) return;
    setTimeout(() => {
      if (this.onlyWhenNoText && (<any>document.activeElement).type === 'text') return;
      this.el.nativeElement.focus();
    });
    this.initialised = true;
  }
}
