import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';

import { SmpConfiguration, SmpConfigurationRecord } from '../../api/SmpConfiguration';

@Injectable()
export class SmpConfigurationService {
  constructor(private http: AuthHttp) {}
  public get(): Observable<SmpConfigurationRecord[]> {
    return this.http.get(this.baseUrl()).map((result) => result.json());
  }

  public getById(id: number): Observable<SmpConfiguration> {
    return this.http
      .get(`${this.baseUrl()}/${id}`)
      .map((result) => result.json());
  }
  public create(
    smpConfiguration: SmpConfiguration
  ): Observable<SmpConfiguration> {
    return this.http
      .post(this.baseUrl(), smpConfiguration)
      .map((result) => result.json());
  }
  public delete(id: number): Observable<boolean> {
    return this.http.delete(`${this.baseUrl()}/${id}`).map(() => true);
  }
  public update(
    id: number,
    smpConfiguration: SmpConfiguration
  ): Observable<boolean> {
    return this.http
      .put(`${this.baseUrl()}/${id}`, smpConfiguration)
      .map(() => true);
  }
  private baseUrl() {
    return '/api/smpconfiguration';
  }
}
