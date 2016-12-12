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
    // this.routes = router.config.filter(route => route.data !== undefined && (!!!route.data['title'] || !!!route.data['root']));
    let routes = this
      .router
      .config
      .map(result => {
        if (!!!result.path) {
          return result.children;
        }

        return result;
      });
    this.routes = this.flatten<Route>(routes).filter(route => !!route.data && !!route.data['title']);

  }
  private flatten<T>(list: Route[]): T[] {
    return list.reduce((a, b) => (Array.isArray(b) ? a.push(...this.flatten(b)) : a.push(b), a), []);
  }
}
