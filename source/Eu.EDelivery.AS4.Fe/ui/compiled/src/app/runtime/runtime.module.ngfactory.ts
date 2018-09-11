/**
 * @fileoverview This file is generated by the Angular template compiler.
 * Do not edit.
 * @suppress {suspiciousCode,uselessCode,missingProperties,missingOverride}
 */
 /* tslint:disable */


import * as i0 from '@angular/core';
import * as i1 from '../../../../src/app/runtime/runtime.module';
import * as i2 from '../authentication/login/login.component.ngfactory';
import * as i3 from '../authentication/unauthorized/unauthorized.component.ngfactory';
import * as i4 from '../common/wrapper.component.ngfactory';
import * as i5 from '../pmodes/receivingpmode/receivingpmode.component.ngfactory';
import * as i6 from '../pmodes/sendingpmode/sendingpmode.component.ngfactory';
import * as i7 from '@angular/forms';
import * as i8 from '@angular/common';
import * as i9 from 'ngx-window-token/dist/src/ngx-window-token';
import * as i10 from 'ngx-clipboard/src/clipboard.service';
import * as i11 from '@angular/platform-browser';
import * as i12 from 'angular2-jwt/angular2-jwt';
import * as i13 from '../../../../src/app/authentication/authentication.module';
import * as i14 from '../../../../src/app/common/spinner/spinner.service';
import * as i15 from '../../../../src/app/common/modal/modal.service';
import * as i16 from '../../../../src/app/common/dialog.service';
import * as i17 from '@angular/http';
import * as i18 from '../../../../src/app/authentication/authentication.store';
import * as i19 from '../../../../src/app/authentication/logout.service';
import * as i20 from '@angular/router';
import * as i21 from '../../../../src/app/authentication/authentication.service';
import * as i22 from '../../../../src/app/setup/setup.guard';
import * as i23 from '../../../../src/app/setup/setup.service';
import * as i24 from '../../../../src/app/common/as4components.module';
import * as i25 from '../../../../src/app/authentication/roles.service';
import * as i26 from '../../../../src/app/common/mustbeauthorized.guard';
import * as i27 from '../../../../src/app/common/candeactivate.guard';
import * as i28 from '../../../../src/app/common/router.service';
import * as i29 from '../../../../src/app/common/form.service';
import * as i30 from '../../../../src/app/common/spinner/customhttp';
import * as i31 from '../../../../src/app/common/error.handler';
import * as i32 from '../../../../src/app/pmodes/pmode.store';
import * as i33 from '../../../../src/app/pmodes/sendingpmode.service';
import * as i34 from '../../../../src/app/settings/runtime.store';
import * as i35 from '../../../../src/app/pmodes/receivingpmode.service';
import * as i36 from 'ngx-clipboard/src/index';
import * as i37 from 'angular2-text-mask/dist/angular2TextMask';
import * as i38 from 'ng2-select2/ng2-select2';
import * as i39 from '../../../../src/app/pmodes/pmodes.module';
import * as i40 from '../../../../src/app/authentication/login/login.component';
import * as i41 from '../../../../src/app/authentication/unauthorized/unauthorized.component';
import * as i42 from '../../../../src/app/common/wrapper.component';
import * as i43 from '../../../../src/app/pmodes/receivingpmode/receivingpmode.component';
import * as i44 from '../../../../src/app/pmodes/sendingpmode/sendingpmode.component';
export const RuntimeModuleNgFactory:i0.NgModuleFactory<i1.RuntimeModule> = i0.ɵcmf(i1.RuntimeModule,
    ([] as any[]),(_l:any) => {
      return i0.ɵmod([i0.ɵmpd(512,i0.ComponentFactoryResolver,i0.ɵCodegenComponentFactoryResolver,
          [[8,[i2.LoginComponentNgFactory,i3.UnauthorizedComponentNgFactory,i4.WrapperComponentNgFactory,
              i5.ReceivingPmodeComponentNgFactory,i6.SendingPmodeComponentNgFactory]],
              [3,i0.ComponentFactoryResolver],i0.NgModuleRef]),i0.ɵmpd(4608,i7.ɵi,
          i7.ɵi,([] as any[])),i0.ɵmpd(4608,i7.FormBuilder,i7.FormBuilder,([] as any[])),
          i0.ɵmpd(4608,i8.NgLocalization,i8.NgLocaleLocalization,[i0.LOCALE_ID]),i0.ɵmpd(5120,
              i9.WINDOW,i9._window,([] as any[])),i0.ɵmpd(5120,i10.ClipboardService,
              i10.CLIPBOARD_SERVICE_PROVIDER_FACTORY,[i11.DOCUMENT,i9.WINDOW,[3,i10.ClipboardService]]),
          i0.ɵmpd(5120,i12.JwtHelper,i13.jwtHelperFactory,([] as any[])),i0.ɵmpd(4608,
              i14.SpinnerService,i14.SpinnerService,([] as any[])),i0.ɵmpd(4608,i15.ModalService,
              i15.ModalService,[i0.ComponentFactoryResolver,i0.Injector]),i0.ɵmpd(4608,
              i16.DialogService,i16.DialogService,[i15.ModalService]),i0.ɵmpd(5120,
              i17.Http,i14.spinnerHttpServiceFactory,[i17.XHRBackend,i17.RequestOptions,
                  i14.SpinnerService,i16.DialogService,i0.Injector]),i0.ɵmpd(4608,
              i18.AuthenticationStore,i18.AuthenticationStore,[i12.JwtHelper]),i0.ɵmpd(4608,
              i19.LogoutService,i19.LogoutService,[i20.Router]),i0.ɵmpd(4608,i21.AuthenticationService,
              i21.AuthenticationService,[i17.Http,i18.AuthenticationStore,i20.Router,
                  i14.SpinnerService,i16.DialogService,i19.LogoutService]),i0.ɵmpd(4608,
              i22.SetupGuard,i22.SetupGuard,[i23.SetupService,i20.Router]),i0.ɵmpd(5120,
              i12.AuthHttp,i24.authHttpServiceFactory,[i17.RequestOptions,i17.XHRBackend,
                  i14.SpinnerService,i16.DialogService,i0.Injector]),i0.ɵmpd(4608,
              i25.RolesService,i25.RolesService,[i12.AuthHttp,i18.AuthenticationStore]),
          i0.ɵmpd(4608,i26.MustBeAuthorizedGuard,i26.MustBeAuthorizedGuard,[i20.Router,
              i25.RolesService,i16.DialogService,i21.AuthenticationService]),i0.ɵmpd(4608,
              i27.CanDeactivateGuard,i27.CanDeactivateGuard,[i16.DialogService]),i0.ɵmpd(4608,
              i28.RouterService,i28.RouterService,[i8.Location,i20.Router]),i0.ɵmpd(4608,
              i29.FormBuilderExtended,i29.FormBuilderExtended,[i7.FormBuilder,i0.Injector]),
          i0.ɵmpd(5120,i30.CustomAuthNoSpinnerHttp,i24.authHttpNoSpinnerServiceFactory,
              [i17.RequestOptions,i17.XHRBackend,i14.SpinnerService,i16.DialogService,
                  i0.Injector]),i0.ɵmpd(5120,i0.ErrorHandler,i31.errorHandlerFactory,
              [i16.DialogService,i14.SpinnerService]),i0.ɵmpd(4608,i32.PmodeStore,
              i32.PmodeStore,([] as any[])),i0.ɵmpd(4608,i33.SendingPmodeService,i33.SendingPmodeService,
              [i12.AuthHttp,i32.PmodeStore,i29.FormBuilderExtended,i34.RuntimeStore]),
          i0.ɵmpd(4608,i35.ReceivingPmodeService,i35.ReceivingPmodeService,[i12.AuthHttp,
              i32.PmodeStore,i29.FormBuilderExtended,i34.RuntimeStore]),i0.ɵmpd(512,
              i7.ɵba,i7.ɵba,([] as any[])),i0.ɵmpd(512,i7.FormsModule,i7.FormsModule,
              ([] as any[])),i0.ɵmpd(512,i7.ReactiveFormsModule,i7.ReactiveFormsModule,
              ([] as any[])),i0.ɵmpd(512,i8.CommonModule,i8.CommonModule,([] as any[])),
          i0.ɵmpd(512,i9.WindowTokenModule,i9.WindowTokenModule,([] as any[])),i0.ɵmpd(512,
              i36.ClipboardModule,i36.ClipboardModule,([] as any[])),i0.ɵmpd(512,i20.RouterModule,
              i20.RouterModule,[[2,i20.ɵa],[2,i20.Router]]),i0.ɵmpd(512,i37.TextMaskModule,
              i37.TextMaskModule,([] as any[])),i0.ɵmpd(512,i38.Select2Module,i38.Select2Module,
              ([] as any[])),i0.ɵmpd(512,i13.AuthenticationModule,i13.AuthenticationModule,
              ([] as any[])),i0.ɵmpd(512,i24.As4ComponentsModule,i24.As4ComponentsModule,
              ([] as any[])),i0.ɵmpd(512,i39.PmodesModule,i39.PmodesModule,([] as any[])),
          i0.ɵmpd(512,i1.RuntimeModule,i1.RuntimeModule,([] as any[])),i0.ɵmpd(1024,
              i20.ROUTES,() => {
                return [[{path:'login',component:i40.LoginComponent,data:{isAuthCheck:false},
                    canActivate:[i22.SetupGuard]},{path:'unauthorized',component:i41.UnauthorizedComponent}],
                    [{path:'pmodes',component:i42.WrapperComponent,children:[{path:'',
                        pathMatch:'full',redirectTo:'receiving',canDeactivate:[i27.CanDeactivateGuard]},
                        {path:'receiving',component:i43.ReceivingPmodeComponent,data:{title:'Receiving PMode',
                            mode:'receiving'},canDeactivate:[i27.CanDeactivateGuard],
                            canActivate:[i26.MustBeAuthorizedGuard]},{path:'receiving/:pmode',
                            component:i43.ReceivingPmodeComponent,data:{title:'Receiving PMode',
                                mode:'receiving',nomenu:true},canDeactivate:[i27.CanDeactivateGuard],
                            canActivate:[i26.MustBeAuthorizedGuard]},{path:'sending',
                            component:i44.SendingPmodeComponent,data:{title:'Sending PMode',
                                mode:'sending'},canDeactivate:[i27.CanDeactivateGuard]},
                        {path:'sending/:pmode',component:i44.SendingPmodeComponent,
                            data:{title:'Sending PMode',mode:'sending',nomenu:true},
                            canDeactivate:[i27.CanDeactivateGuard]}],data:{title:'PModes'},
                        canActivate:[i26.MustBeAuthorizedGuard]}]];
              },([] as any[]))]);
    });
//# sourceMappingURL=data:application/json;base64,eyJmaWxlIjoiQzovRGV2L2NvZGl0LnZpc3VhbHN0dWRpby5jb20vQVM0Lk5FVC9zb3VyY2UvRmUvRXUuRURlbGl2ZXJ5LkFTNC5GZS91aS9zcmMvYXBwL3J1bnRpbWUvcnVudGltZS5tb2R1bGUubmdmYWN0b3J5LnRzIiwidmVyc2lvbiI6Mywic291cmNlUm9vdCI6IiIsInNvdXJjZXMiOlsibmc6Ly8vQzovRGV2L2NvZGl0LnZpc3VhbHN0dWRpby5jb20vQVM0Lk5FVC9zb3VyY2UvRmUvRXUuRURlbGl2ZXJ5LkFTNC5GZS91aS9zcmMvYXBwL3J1bnRpbWUvcnVudGltZS5tb2R1bGUudHMiXSwic291cmNlc0NvbnRlbnQiOlsiICJdLCJtYXBwaW5ncyI6IkFBQUE7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7OyJ9