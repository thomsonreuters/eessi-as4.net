import 'core-js/es7/reflect';

export function isDate() {
    return (<any>Reflect).metadata('isDate', 'true');
}

export function getIsDate(target: Object, key: string | symbol) {
    return (<any>Reflect).getMetadata('isDate', target, key);
};
