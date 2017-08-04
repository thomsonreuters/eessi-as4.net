import { Router, Route } from '@angular/router';
import { Component, ViewEncapsulation, ChangeDetectionStrategy } from '@angular/core';

import { RolesService } from './../../authentication/roles.service';

@Component({
  selector: 'as4-sidebar',
  encapsulation: ViewEncapsulation.None,
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SidebarComponent {
  public routes: Route[];
  constructor(private _router: Router, private _rolesService: RolesService) {
    let routes = this
      ._router
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
      .filter((route) => this.validateAuth(route.data) && !!route.data && !!route.data['title']);

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
  private validateAuth(data: any): boolean {
    if (!!!data) {
      return true;
    }

    return !!!data['roles'] || this._rolesService.validate(data['roles']);
  }
}
