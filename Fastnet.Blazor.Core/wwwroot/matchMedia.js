"use strict";
////interface MediaQueryList extends EventTarget {
////    readonly matches: boolean;
////    readonly media: string;
////    onchange: ((this: MediaQueryList, ev: MediaQueryListEvent) => any) | null;
////    /** @deprecated */
////    addListener(listener: ((this: MediaQueryList, ev: MediaQueryListEvent) => any) | null): void;
////    /** @deprecated */
////    removeListener(listener: ((this: MediaQueryList, ev: MediaQueryListEvent) => any) | null): void;
////    addEventListener<K extends keyof MediaQueryListEventMap>(type: K, listener: (this: MediaQueryList, ev: MediaQueryListEventMap[K]) => any, options?: boolean | AddEventListenerOptions): void;
////    addEventListener(type: string, listener: EventListenerOrEventListenerObject, options?: boolean | AddEventListenerOptions): void;
////    removeEventListener<K extends keyof MediaQueryListEventMap>(type: K, listener: (this: MediaQueryList, ev: MediaQueryListEventMap[K]) => any, options?: boolean | EventListenerOptions): void;
////    removeEventListener(type: string, listener: EventListenerOrEventListenerObject, options?: boolean | EventListenerOptions): void;
////}
Object.defineProperty(exports, "__esModule", { value: true });
exports.mm_removeNofication = exports.mm_notifyChange = void 0;
function mm_notifyChange() {
    var mediaQuery = window.matchMedia('(min-width: 480px)');
    mediaQuery.addEventListener("change", function (e) {
        if (e.matches) {
            console.log('minimum width is 480px - phone?');
        }
        else {
            console.log('minimum width is greater then 480px - not a phone?');
        }
    });
    return mediaQuery;
}
exports.mm_notifyChange = mm_notifyChange;
function mm_removeNofication(mediaQuery) {
    removeEventListener("change", mediaQuery, false);
}
exports.mm_removeNofication = mm_removeNofication;
/*
 * @media (min-width: 1280px)                                   - desktops, screen 1281px or higher
 * @media (min-width: 1024px) and (max-width: 1279px)           - laptops, desktops, screens 1025px to 1280px
 * @media (min-width: 768px) and (max-width: 1023px) and (orientation: portrait) - tablets, ipads (portrait), screens between 768px and 1024px
 * @media (min-width: 768px) and (max-width: 1023px) and (orientation: landscape) - tablets, ipads (landscape), screens between 768px and 1024px
 * @media (min-width: 480px) and (max-width: 767px)             - low resolution tablets, mobiles (landscape), screens between 481px to 767px
 * @media (min-width: 320px) and (max-width: 479px)             - most smart phones (portrait), screens between 320px and 479px
 */ 
//# sourceMappingURL=matchMedia.js.map