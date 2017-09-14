import { Router, Route } from '@angular/router';
import { Component, ViewEncapsulation, ChangeDetectionStrategy } from '@angular/core';

import { RolesService } from './../../authentication/roles.service';

interface IRoute {
  path: string;
  data: any;
  children: Array<IRoute | null> | null;
}

@Component({
  selector: 'as4-sidebar',
  encapsulation: ViewEncapsulation.None,
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SidebarComponent {
  public routes: Array<IRoute | null>;
  constructor(private _router: Router, private _rolesService: RolesService) {
    let convert = (route: Route, parent: Route | null): IRoute | null => {
      if (!!!route.path && !!!parent) {
        return null;
      }
      return {
        path: !!!route.path ? parent!.path! : route.path,
        data: route.data,
        children: !!!route.children ? null : route.children.map((child) => convert(child, route))
      };
    };

    let routes = this
      ._router
      .config
      .map((result) => {
        if (!!!result.data && !!result.children) {
          return result.children.map((child) => convert(child, result)).filter((child) => !!child && !!child.data);
        }

        return [convert(result, null)];
      });

    if (!!!routes || routes.length === 0) {
      this.routes = new Array<IRoute>();
      return;
    }

    let data = routes
      .reduce((a, b) => a.concat(b))
      .filter((route) => !!route && !!route.data && !!route.data['title'] && this.validateAuth(route.data));

    this.routes = data
      .sort((a, b) => {
        let aWeight = !!!a!.data!['weight'] ? 0 : a!.data!['weight'];
        let bWeight = !!!b!.data!['weight'] ? 0 : b!.data!['weight'];
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
