import 'core-js/es7/reflect';

const flag = 'noquery';

export function noQuery() {
    return (<any>Reflect).metadata(flag, 'true');
}

export function getNoQuery(target: Object, key: string | symbol) {
    return (<any>Reflect).getMetadata(flag, target, key);
};
