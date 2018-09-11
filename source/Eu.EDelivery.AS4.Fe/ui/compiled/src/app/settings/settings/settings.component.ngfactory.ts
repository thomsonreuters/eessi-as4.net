/**
 * @fileoverview This file is generated by the Angular template compiler.
 * Do not edit.
 * @suppress {suspiciousCode,uselessCode,missingProperties,missingOverride}
 */
 /* tslint:disable */


import * as i0 from '@angular/core';
import * as i1 from '../../common/box/box.component.ngfactory';
import * as i2 from '../../../../../src/app/common/box/box.component';
import * as i3 from '../base.component.ngfactory';
import * as i4 from '../../../../../src/app/settings/base.component';
import * as i5 from '../../../../../src/app/settings/settings.service';
import * as i6 from '@angular/forms';
import * as i7 from '../../../../../src/app/settings/runtime.store';
import * as i8 from '../../../../../src/app/common/dialog.service';
import * as i9 from '@angular/common';
import * as i10 from '../database.component.ngfactory';
import * as i11 from '../../../../../src/app/settings/database.component';
import * as i12 from '../authorizationmap/authorizationmap.component.ngfactory';
import * as i13 from '../../../../../src/app/settings/authorizationmap/authorizationmap.component';
import * as i14 from '../../../../../src/app/settings/authorizationmap/authorizationmapservice';
import * as i15 from '../commonsettings.component.ngfactory';
import * as i16 from '../../../../../src/app/settings/commonsettings.component';
import * as i17 from '../../../../../src/app/settings/settings/settings.component';
import * as i18 from '../../../../../src/app/settings/settings.store';
const styles_SettingsComponent:any[] = ([] as any[]);
export const RenderType_SettingsComponent:i0.RendererType2 = i0.ɵcrt({encapsulation:2,
    styles:styles_SettingsComponent,data:{}});
export function View_SettingsComponent_0(_l:any):i0.ɵViewDefinition {
  return i0.ɵvid(0,[i0.ɵqud(671088640,1,{components:1}),(_l()(),i0.ɵeld(1,0,(null as any),
      (null as any),15,'as4-box',([] as any[]),(null as any),(null as any),(null as any),
      i1.View_BoxComponent_0,i1.RenderType_BoxComponent)),i0.ɵdid(2,114688,(null as any),
      0,i2.BoxComponent,[i0.ElementRef,i0.Renderer],{title:[0,'title']},(null as any)),
      (_l()(),i0.ɵted(-1,2,['\n    '])),(_l()(),i0.ɵeld(4,0,(null as any),1,4,'div',
          [['content','']],(null as any),(null as any),(null as any),(null as any),
          (null as any))),(_l()(),i0.ɵted(-1,(null as any),['\n        '])),(_l()(),
          i0.ɵeld(6,0,(null as any),(null as any),1,'as4-base-settings',([] as any[]),
              (null as any),(null as any),(null as any),i3.View_BaseSettingsComponent_0,
              i3.RenderType_BaseSettingsComponent)),i0.ɵdid(7,49152,[[1,4],['baseSettings',
          4],['dirtycheck',4]],0,i4.BaseSettingsComponent,[i5.SettingsService,i6.FormBuilder,
          i7.RuntimeStore,i8.DialogService],{settings:[0,'settings']},(null as any)),
      (_l()(),i0.ɵted(-1,(null as any),['\n    '])),(_l()(),i0.ɵted(-1,2,['\n    '])),
      (_l()(),i0.ɵeld(10,0,(null as any),0,5,'button',[['action',''],['class','btn btn-box-tool'],
          ['data-cy','save'],['type','button']],[[8,'disabled',0]],[[(null as any),
          'click']],(_v,en,$event) => {
        var ad:boolean = true;
        if (('click' === en)) {
          const pd_0:any = ((<any>i0.ɵnov(_v,7).save()) !== false);
          ad = (pd_0 && ad);
        }
        return ad;
      },(null as any),(null as any))),i0.ɵpid(131072,i9.AsyncPipe,[i0.ChangeDetectorRef]),
      (_l()(),i0.ɵted(-1,(null as any),['\n        '])),(_l()(),i0.ɵeld(13,0,(null as any),
          (null as any),1,'i',[['class','fa fa-save']],[[2,'active',(null as any)]],
          (null as any),(null as any),(null as any),(null as any))),i0.ɵpid(131072,
          i9.AsyncPipe,[i0.ChangeDetectorRef]),(_l()(),i0.ɵted(-1,(null as any),['\n    '])),
      (_l()(),i0.ɵted(-1,2,['\n'])),(_l()(),i0.ɵted(-1,(null as any),['\n'])),(_l()(),
          i0.ɵeld(18,0,(null as any),(null as any),15,'as4-box',([] as any[]),(null as any),
              (null as any),(null as any),i1.View_BoxComponent_0,i1.RenderType_BoxComponent)),
      i0.ɵdid(19,114688,(null as any),0,i2.BoxComponent,[i0.ElementRef,i0.Renderer],
          {title:[0,'title']},(null as any)),(_l()(),i0.ɵted(-1,2,['\n    '])),(_l()(),
          i0.ɵeld(21,0,(null as any),1,4,'div',[['content','']],(null as any),(null as any),
              (null as any),(null as any),(null as any))),(_l()(),i0.ɵted(-1,(null as any),
          ['\n        '])),(_l()(),i0.ɵeld(23,0,(null as any),(null as any),1,'as4-database-settings',
          ([] as any[]),(null as any),(null as any),(null as any),i10.View_DatabaseSettingsComponent_0,
          i10.RenderType_DatabaseSettingsComponent)),i0.ɵdid(24,49152,[[1,4],['databaseSettings',
          4],['dirtycheck',4]],0,i11.DatabaseSettingsComponent,[i5.SettingsService,
          i6.FormBuilder,i8.DialogService],{settings:[0,'settings']},(null as any)),
      (_l()(),i0.ɵted(-1,(null as any),['\n    '])),(_l()(),i0.ɵted(-1,2,['\n    '])),
      (_l()(),i0.ɵeld(27,0,(null as any),0,5,'button',[['action',''],['class','btn btn-box-tool'],
          ['type','button']],[[8,'disabled',0]],[[(null as any),'click']],(_v,en,$event) => {
        var ad:boolean = true;
        if (('click' === en)) {
          const pd_0:any = ((<any>i0.ɵnov(_v,24).save()) !== false);
          ad = (pd_0 && ad);
        }
        return ad;
      },(null as any),(null as any))),i0.ɵpid(131072,i9.AsyncPipe,[i0.ChangeDetectorRef]),
      (_l()(),i0.ɵted(-1,(null as any),['\n        '])),(_l()(),i0.ɵeld(30,0,(null as any),
          (null as any),1,'i',[['class','fa fa-save']],[[2,'active',(null as any)]],
          (null as any),(null as any),(null as any),(null as any))),i0.ɵpid(131072,
          i9.AsyncPipe,[i0.ChangeDetectorRef]),(_l()(),i0.ɵted(-1,(null as any),['\n    '])),
      (_l()(),i0.ɵted(-1,2,['\n'])),(_l()(),i0.ɵted(-1,(null as any),['\n'])),(_l()(),
          i0.ɵeld(35,0,(null as any),(null as any),15,'as4-box',[['title','Authorization map']],
              (null as any),(null as any),(null as any),i1.View_BoxComponent_0,i1.RenderType_BoxComponent)),
      i0.ɵdid(36,114688,(null as any),0,i2.BoxComponent,[i0.ElementRef,i0.Renderer],
          {title:[0,'title']},(null as any)),(_l()(),i0.ɵted(-1,2,['\n    '])),(_l()(),
          i0.ɵeld(38,0,(null as any),1,4,'div',[['content','']],(null as any),(null as any),
              (null as any),(null as any),(null as any))),(_l()(),i0.ɵted(-1,(null as any),
          ['\n        '])),(_l()(),i0.ɵeld(40,0,(null as any),(null as any),1,'as4-authorizationmap',
          ([] as any[]),(null as any),(null as any),(null as any),i12.View_AuthorizationMapComponent_0,
          i12.RenderType_AuthorizationMapComponent)),i0.ɵdid(41,49152,[['authorizationmap',
          4]],0,i13.AuthorizationMapComponent,[i14.AuthorizationMapService,i8.DialogService,
          i6.FormBuilder],(null as any),(null as any)),(_l()(),i0.ɵted(-1,(null as any),
          ['\n    '])),(_l()(),i0.ɵted(-1,2,['\n    '])),(_l()(),i0.ɵeld(44,0,(null as any),
          0,5,'button',[['action',''],['class','btn btn-box-tool'],['type','button']],
          [[1,'disabled',0]],[[(null as any),'click']],(_v,en,$event) => {
            var ad:boolean = true;
            if (('click' === en)) {
              const pd_0:any = ((<any>i0.ɵnov(_v,41).save()) !== false);
              ad = (pd_0 && ad);
            }
            return ad;
          },(null as any),(null as any))),i0.ɵpid(131072,i9.AsyncPipe,[i0.ChangeDetectorRef]),
      (_l()(),i0.ɵted(-1,(null as any),['\n        '])),(_l()(),i0.ɵeld(47,0,(null as any),
          (null as any),1,'i',[['class','fa fa-save']],[[2,'active',(null as any)]],
          (null as any),(null as any),(null as any),(null as any))),i0.ɵpid(131072,
          i9.AsyncPipe,[i0.ChangeDetectorRef]),(_l()(),i0.ɵted(-1,(null as any),['\n    '])),
      (_l()(),i0.ɵted(-1,2,['\n'])),(_l()(),i0.ɵted(-1,(null as any),['\n'])),(_l()(),
          i0.ɵeld(52,0,(null as any),(null as any),15,'as4-box',([] as any[]),(null as any),
              (null as any),(null as any),i1.View_BoxComponent_0,i1.RenderType_BoxComponent)),
      i0.ɵdid(53,114688,(null as any),0,i2.BoxComponent,[i0.ElementRef,i0.Renderer],
          {title:[0,'title']},(null as any)),(_l()(),i0.ɵted(-1,2,['\n    '])),(_l()(),
          i0.ɵeld(55,0,(null as any),1,4,'div',[['content','']],(null as any),(null as any),
              (null as any),(null as any),(null as any))),(_l()(),i0.ɵted(-1,(null as any),
          ['\n        '])),(_l()(),i0.ɵeld(57,0,(null as any),(null as any),1,'as4-custom-settings',
          ([] as any[]),(null as any),(null as any),(null as any),i15.View_CommonSettingsComponent_0,
          i15.RenderType_CommonSettingsComponent)),i0.ɵdid(58,49152,[[1,4],['customSettings',
          4],['dirtycheck',4]],0,i16.CommonSettingsComponent,[i5.SettingsService,i6.FormBuilder,
          i8.DialogService],{settings:[0,'settings']},(null as any)),(_l()(),i0.ɵted(-1,
          (null as any),['\n    '])),(_l()(),i0.ɵted(-1,2,['\n    '])),(_l()(),i0.ɵeld(61,
          0,(null as any),0,5,'button',[['action',''],['class','btn btn-box-tool'],
              ['type','button']],[[8,'disabled',0]],[[(null as any),'click']],(_v,
              en,$event) => {
            var ad:boolean = true;
            if (('click' === en)) {
              const pd_0:any = ((<any>i0.ɵnov(_v,58).save()) !== false);
              ad = (pd_0 && ad);
            }
            return ad;
          },(null as any),(null as any))),i0.ɵpid(131072,i9.AsyncPipe,[i0.ChangeDetectorRef]),
      (_l()(),i0.ɵted(-1,(null as any),['\n        '])),(_l()(),i0.ɵeld(64,0,(null as any),
          (null as any),1,'i',[['class','fa fa-save']],[[2,'active',(null as any)]],
          (null as any),(null as any),(null as any),(null as any))),i0.ɵpid(131072,
          i9.AsyncPipe,[i0.ChangeDetectorRef]),(_l()(),i0.ɵted(-1,(null as any),['\n    '])),
      (_l()(),i0.ɵted(-1,2,['\n']))],(_ck,_v) => {
    var _co:i17.SettingsComponent = _v.component;
    const currVal_0:any = 'Basic settings';
    _ck(_v,2,0,currVal_0);
    const currVal_1:any = _co.settings;
    _ck(_v,7,0,currVal_1);
    const currVal_4:any = 'ConnectionString settings';
    _ck(_v,19,0,currVal_4);
    const currVal_5:any = (_co.settings && _co.settings.database);
    _ck(_v,24,0,currVal_5);
    const currVal_8:any = 'Authorization map';
    _ck(_v,36,0,currVal_8);
    const currVal_11:any = 'Custom settings';
    _ck(_v,53,0,currVal_11);
    const currVal_12:any = (_co.settings && _co.settings.customSettings);
    _ck(_v,58,0,currVal_12);
  },(_ck,_v) => {
    const currVal_2:boolean = !i0.ɵunv(_v,10,0,i0.ɵnov(_v,11).transform(i0.ɵnov(_v,
        7).isDirty));
    _ck(_v,10,0,currVal_2);
    const currVal_3:any = i0.ɵunv(_v,13,0,i0.ɵnov(_v,14).transform(i0.ɵnov(_v,7).isDirty));
    _ck(_v,13,0,currVal_3);
    const currVal_6:boolean = !i0.ɵunv(_v,27,0,i0.ɵnov(_v,28).transform(i0.ɵnov(_v,
        24).isDirty));
    _ck(_v,27,0,currVal_6);
    const currVal_7:any = i0.ɵunv(_v,30,0,i0.ɵnov(_v,31).transform(i0.ɵnov(_v,24).isDirty));
    _ck(_v,30,0,currVal_7);
    const currVal_9:any = (i0.ɵunv(_v,44,0,i0.ɵnov(_v,45).transform(i0.ɵnov(_v,41).isSaveEnabled))? (null as any): true);
    _ck(_v,44,0,currVal_9);
    const currVal_10:any = i0.ɵunv(_v,47,0,i0.ɵnov(_v,48).transform(i0.ɵnov(_v,41).isSaveEnabled));
    _ck(_v,47,0,currVal_10);
    const currVal_13:any = (i0.ɵunv(_v,61,0,i0.ɵnov(_v,62).transform(i0.ɵnov(_v,58).isSaveEnabled))? (null as any): true);
    _ck(_v,61,0,currVal_13);
    const currVal_14:any = i0.ɵunv(_v,64,0,i0.ɵnov(_v,65).transform(i0.ɵnov(_v,58).isSaveEnabled));
    _ck(_v,64,0,currVal_14);
  });
}
export function View_SettingsComponent_Host_0(_l:any):i0.ɵViewDefinition {
  return i0.ɵvid(0,[(_l()(),i0.ɵeld(0,0,(null as any),(null as any),1,'as4-settings',
      ([] as any[]),(null as any),(null as any),(null as any),View_SettingsComponent_0,
      RenderType_SettingsComponent)),i0.ɵdid(1,180224,(null as any),0,i17.SettingsComponent,
      [i18.SettingsStore,i5.SettingsService,i0.ElementRef],(null as any),(null as any))],
      (null as any),(null as any));
}
export const SettingsComponentNgFactory:i0.ComponentFactory<i17.SettingsComponent> = i0.ɵccf('as4-settings',
    i17.SettingsComponent,View_SettingsComponent_Host_0,{},{},([] as any[]));
//# sourceMappingURL=data:application/json;base64,eyJmaWxlIjoiQzovRGV2L2NvZGl0LnZpc3VhbHN0dWRpby5jb20vQVM0Lk5FVC9zb3VyY2UvRmUvRXUuRURlbGl2ZXJ5LkFTNC5GZS91aS9zcmMvYXBwL3NldHRpbmdzL3NldHRpbmdzL3NldHRpbmdzLmNvbXBvbmVudC5uZ2ZhY3RvcnkudHMiLCJ2ZXJzaW9uIjozLCJzb3VyY2VSb290IjoiIiwic291cmNlcyI6WyJuZzovLy9DOi9EZXYvY29kaXQudmlzdWFsc3R1ZGlvLmNvbS9BUzQuTkVUL3NvdXJjZS9GZS9FdS5FRGVsaXZlcnkuQVM0LkZlL3VpL3NyYy9hcHAvc2V0dGluZ3Mvc2V0dGluZ3Mvc2V0dGluZ3MuY29tcG9uZW50LnRzIiwibmc6Ly8vQzovRGV2L2NvZGl0LnZpc3VhbHN0dWRpby5jb20vQVM0Lk5FVC9zb3VyY2UvRmUvRXUuRURlbGl2ZXJ5LkFTNC5GZS91aS9zcmMvYXBwL3NldHRpbmdzL3NldHRpbmdzL3NldHRpbmdzLmNvbXBvbmVudC5odG1sIiwibmc6Ly8vQzovRGV2L2NvZGl0LnZpc3VhbHN0dWRpby5jb20vQVM0Lk5FVC9zb3VyY2UvRmUvRXUuRURlbGl2ZXJ5LkFTNC5GZS91aS9zcmMvYXBwL3NldHRpbmdzL3NldHRpbmdzL3NldHRpbmdzLmNvbXBvbmVudC50cy5TZXR0aW5nc0NvbXBvbmVudF9Ib3N0Lmh0bWwiXSwic291cmNlc0NvbnRlbnQiOlsiICIsIjxhczQtYm94IFt0aXRsZV09XCInQmFzaWMgc2V0dGluZ3MnXCI+XHJcbiAgICA8ZGl2IGNvbnRlbnQ+XHJcbiAgICAgICAgPGFzNC1iYXNlLXNldHRpbmdzIFtzZXR0aW5nc109XCJzZXR0aW5nc1wiICNiYXNlU2V0dGluZ3MgI2RpcnR5Y2hlY2s+PC9hczQtYmFzZS1zZXR0aW5ncz5cclxuICAgIDwvZGl2PlxyXG4gICAgPGJ1dHRvbiBkYXRhLWN5PVwic2F2ZVwiIFtkaXNhYmxlZF09XCIhKGJhc2VTZXR0aW5ncy5pc0RpcnR5IHwgYXN5bmMpXCIgYWN0aW9uIHR5cGU9XCJidXR0b25cIiBjbGFzcz1cImJ0biBidG4tYm94LXRvb2xcIiAoY2xpY2spPVwiYmFzZVNldHRpbmdzLnNhdmUoKVwiPlxyXG4gICAgICAgIDxpIGNsYXNzPVwiZmEgZmEtc2F2ZVwiIFtjbGFzcy5hY3RpdmVdPVwiYmFzZVNldHRpbmdzLmlzRGlydHkgfCBhc3luY1wiPjwvaT5cclxuICAgIDwvYnV0dG9uPlxyXG48L2FzNC1ib3g+XHJcbjxhczQtYm94IFt0aXRsZV09XCInQ29ubmVjdGlvblN0cmluZyBzZXR0aW5ncydcIj5cclxuICAgIDxkaXYgY29udGVudD5cclxuICAgICAgICA8YXM0LWRhdGFiYXNlLXNldHRpbmdzIFtzZXR0aW5nc109XCJzZXR0aW5ncyAmJiBzZXR0aW5ncy5kYXRhYmFzZVwiICNkYXRhYmFzZVNldHRpbmdzICNkaXJ0eWNoZWNrPjwvYXM0LWRhdGFiYXNlLXNldHRpbmdzPlxyXG4gICAgPC9kaXY+XHJcbiAgICA8YnV0dG9uIFtkaXNhYmxlZF09XCIhKGRhdGFiYXNlU2V0dGluZ3MuaXNEaXJ0eSB8IGFzeW5jKVwiIGFjdGlvbiB0eXBlPVwiYnV0dG9uXCIgY2xhc3M9XCJidG4gYnRuLWJveC10b29sXCIgKGNsaWNrKT1cImRhdGFiYXNlU2V0dGluZ3Muc2F2ZSgpXCI+XHJcbiAgICAgICAgPGkgY2xhc3M9XCJmYSBmYS1zYXZlXCIgW2NsYXNzLmFjdGl2ZV09XCJkYXRhYmFzZVNldHRpbmdzLmlzRGlydHkgfCBhc3luY1wiPjwvaT5cclxuICAgIDwvYnV0dG9uPlxyXG48L2FzNC1ib3g+XHJcbjxhczQtYm94IHRpdGxlPVwiQXV0aG9yaXphdGlvbiBtYXBcIj5cclxuICAgIDxkaXYgY29udGVudD5cclxuICAgICAgICA8YXM0LWF1dGhvcml6YXRpb25tYXAgI2F1dGhvcml6YXRpb25tYXA+PC9hczQtYXV0aG9yaXphdGlvbm1hcD5cclxuICAgIDwvZGl2PlxyXG4gICAgPGJ1dHRvbiBbYXR0ci5kaXNhYmxlZF09XCIoYXV0aG9yaXphdGlvbm1hcC5pc1NhdmVFbmFibGVkIHwgYXN5bmMpID8gbnVsbCA6IHRydWVcIiBhY3Rpb24gdHlwZT1cImJ1dHRvblwiIGNsYXNzPVwiYnRuIGJ0bi1ib3gtdG9vbFwiXHJcbiAgICAgICAgKGNsaWNrKT1cImF1dGhvcml6YXRpb25tYXAuc2F2ZSgpXCI+XHJcbiAgICAgICAgPGkgY2xhc3M9XCJmYSBmYS1zYXZlXCIgW2NsYXNzLmFjdGl2ZV09XCJhdXRob3JpemF0aW9ubWFwLmlzU2F2ZUVuYWJsZWQgfCBhc3luY1wiPjwvaT5cclxuICAgIDwvYnV0dG9uPlxyXG48L2FzNC1ib3g+XHJcbjxhczQtYm94IFt0aXRsZV09XCInQ3VzdG9tIHNldHRpbmdzJ1wiPlxyXG4gICAgPGRpdiBjb250ZW50PlxyXG4gICAgICAgIDxhczQtY3VzdG9tLXNldHRpbmdzIFtzZXR0aW5nc109XCJzZXR0aW5ncyAmJiBzZXR0aW5ncy5jdXN0b21TZXR0aW5nc1wiICNjdXN0b21TZXR0aW5ncyAjZGlydHljaGVjaz48L2FzNC1jdXN0b20tc2V0dGluZ3M+XHJcbiAgICA8L2Rpdj5cclxuICAgIDxidXR0b24gW2Rpc2FibGVkXT1cIihjdXN0b21TZXR0aW5ncy5pc1NhdmVFbmFibGVkIHwgYXN5bmMpID8gbnVsbCA6IHRydWVcIiBhY3Rpb24gdHlwZT1cImJ1dHRvblwiIGNsYXNzPVwiYnRuIGJ0bi1ib3gtdG9vbFwiIChjbGljayk9XCJjdXN0b21TZXR0aW5ncy5zYXZlKClcIj5cclxuICAgICAgICA8aSBjbGFzcz1cImZhIGZhLXNhdmVcIiBbY2xhc3MuYWN0aXZlXT1cImN1c3RvbVNldHRpbmdzLmlzU2F2ZUVuYWJsZWQgfCBhc3luY1wiPjwvaT5cclxuICAgIDwvYnV0dG9uPlxyXG48L2FzNC1ib3g+IiwiPGFzNC1zZXR0aW5ncz48L2FzNC1zZXR0aW5ncz4iXSwibWFwcGluZ3MiOiJBQUFBOzs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7O3dEQ0FBO01BQUE7dURBQUEsVUFBQTtNQUFBO01BQW9DLGtDQUNoQztVQUFBO1VBQUEsZ0JBQWEsa0RBQ1Q7aUJBQUE7Y0FBQTtpREFBQSxVQUFBO1VBQUE7MENBQUE7TUFBdUYsOENBQ3JGO01BQ047VUFBQTtVQUFBO1FBQUE7UUFBa0g7VUFBQTtVQUFBO1FBQUE7UUFBbEg7TUFBQSx1Q0FBdUI7TUFBeUgsa0RBQzVJO1VBQUE7VUFBQSxpRUFBc0I7dUJBQUEsd0JBQWtEO01BQ25FLDhCQUNILDBDQUNWO2lCQUFBO2NBQUE7YUFBQTtVQUFBLG1DQUErQyxrQ0FDM0M7aUJBQUE7Y0FBQSw0Q0FBYTtVQUFBLGlCQUNUO1VBQUE7a0RBQUEsVUFBQTtVQUFBO3lDQUFBO01BQXdILDhDQUN0SDtNQUNOO1VBQUE7UUFBQTtRQUF1RztVQUFBO1VBQUE7UUFBQTtRQUF2RztNQUFBLHVDQUFRO01BQWlJLGtEQUNySTtVQUFBO1VBQUEsaUVBQXNCO3VCQUFBLHdCQUFzRDtNQUN2RSw4QkFDSCwwQ0FDVjtpQkFBQTtjQUFBO2FBQUE7VUFBQSxtQ0FBbUMsa0NBQy9CO2lCQUFBO2NBQUEsNENBQWE7VUFBQSxpQkFDVDtVQUFBO2tEQUFBLFVBQUE7VUFBQTt3QkFBQSwrQkFBK0Q7VUFBQSxhQUM3RCxrQ0FDTjtVQUFBO1VBQUE7WUFBQTtZQUNJO2NBQUE7Y0FBQTtZQUFBO1lBREo7VUFBQSx1Q0FBUTtNQUM4QixrREFDbEM7VUFBQTtVQUFBLGlFQUFzQjt1QkFBQSx3QkFBNEQ7TUFDN0UsOEJBQ0gsMENBQ1Y7aUJBQUE7Y0FBQTthQUFBO1VBQUEsbUNBQXFDLGtDQUNqQztpQkFBQTtjQUFBLDRDQUFhO1VBQUEsaUJBQ1Q7VUFBQTtnREFBQSxVQUFBO1VBQUE7MEJBQUEsMkNBQXdIO1VBQUEsMkJBQ3RILGtDQUNOO1VBQUE7Y0FBQTt1QkFBQTtZQUFBO1lBQXdIO2NBQUE7Y0FBQTtZQUFBO1lBQXhIO1VBQUEsdUNBQVE7TUFBZ0osa0RBQ3BKO1VBQUE7VUFBQSxpRUFBc0I7dUJBQUEsd0JBQTBEO01BQzNFOztJQS9CSjtJQUFULFdBQVMsU0FBVDtJQUUyQjtJQUFuQixXQUFtQixTQUFuQjtJQU1DO0lBQVQsWUFBUyxTQUFUO0lBRStCO0lBQXZCLFlBQXVCLFNBQXZCO0lBTUM7SUFBVCxZQUFTLFNBQVQ7SUFTUztJQUFULFlBQVMsVUFBVDtJQUU2QjtJQUFyQixZQUFxQixVQUFyQjs7SUF2Qm1CO1FBQUE7SUFBdkIsWUFBdUIsU0FBdkI7SUFDMEI7SUFBdEIsWUFBc0IsU0FBdEI7SUFPSTtRQUFBO0lBQVIsWUFBUSxTQUFSO0lBQzBCO0lBQXRCLFlBQXNCLFNBQXRCO0lBT0k7SUFBUixZQUFRLFNBQVI7SUFFMEI7SUFBdEIsWUFBc0IsVUFBdEI7SUFPSTtJQUFSLFlBQVEsVUFBUjtJQUMwQjtJQUF0QixZQUFzQixVQUF0Qjs7OztvQkM5QlI7TUFBQTtrQ0FBQSxVQUFBO01BQUE7Ozs7In0=