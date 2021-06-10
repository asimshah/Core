export function localStorage_clear() {
    localStorage.clear();
}
export function localStorage_set(key, data) {
    localStorage.setItem(key, data);
}
export function localStorage_get(key) {
    return localStorage.getItem(key);
}
export function localStorage_remove(key) {
    localStorage.removeItem(key);
}
export function localStorage_contains(key) {
    return localStorage.hasOwnProperty(key);
}


