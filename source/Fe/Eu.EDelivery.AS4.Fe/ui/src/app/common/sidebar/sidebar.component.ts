import { Router, Route } from '@angular/router';
import { Component, ViewEncapsulation, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'as4-sidebar',
  encapsulation: ViewEncapsulation.None,
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SidebarComponent {
  public routes: Route[];
  constructor(private router: Router) {
    let routes = this
      .router
      .config
      .map((result) => {
        if (!!!result.path) {
          return result.children;
        }

        return [result];
      });
    this.routes = routes
      .reduce((a, b) => a.concat(b))
      .filter((route) => !!route.data && !!route.data['title'])
      .sort((route) => +route.data['weight']);
  }
}
