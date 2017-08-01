export function jsonAccessor(data: any, path: string, defaultValue: any = null) {
    if (!!!data) {
        return defaultValue;
    }
    const splitted = path.split('.');
    let result = access(data, splitted, 0);
    if (result === undefined || result === null) {
        return defaultValue;
    }
    return result;
}

function access(data: any, path: string[], index: number = 0): any | null {
    if (index === (path.length - 1)) {
        return data[path[index]];
    }

    if (data[path[index]] === null || data[path[index]] === undefined) {
        return null;
    }


    return access(data[path[index]], path, ++index);
}
