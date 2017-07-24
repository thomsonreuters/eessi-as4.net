import { Pipe, PipeTransform, Sanitizer, SecurityContext } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import * as vkbeautify from 'vkbeautify';

@Pipe({
    name: 'asxml'
})
export class AsXmlPipe implements PipeTransform {
    constructor(private _sanitizer: DomSanitizer) { }
    public transform(inp: string): SafeHtml {
        return vkbeautify.xml(inp);
    }
}
