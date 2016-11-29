import { Router, Route } from '@angular/router';
import { Component, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'as4-sidebar',
  encapsulation: ViewEncapsulation.None,
  templateUrl: './sidebar.component.html'
})
export class SidebarComponent {
  public routes: Array<Route>;
  constructor(private router: Router) {
    this.routes = router.config.filter(route => route.data !== undefined && route.data['title'] !== undefined);
  }
}
