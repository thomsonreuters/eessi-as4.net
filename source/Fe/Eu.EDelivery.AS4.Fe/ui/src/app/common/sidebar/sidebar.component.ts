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

    if (!!!routes || routes.length === 0) {
      this.routes = new Array<Route>();
      return;
    }

    let data = routes
      .reduce((a, b) => a!.concat(b!))!
      .filter((route) => !!route.data && !!route.data['title']);

    this.routes = data
      .sort((a, b) => {
        let aWeight = !!!a.data!['weight'] ? 0 : a.data!['weight'];
        let bWeight = !!!b.data!['weight'] ? 0 : b.data!['weight'];

        // tslint:disable-next-line:curly
        if (aWeight < bWeight) return -1;
        // tslint:disable-next-line:curly
        else if (aWeight > bWeight) return 1;
        // tslint:disable-next-line:curly
        else return 0;
      });
  }
}
