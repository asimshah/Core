export function tools_saveAsFile(filename, bytesBase64) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + bytesBase64;
    document.body.appendChild(link); // Needed for Firefox
    link.click();
    document.body.removeChild(link);
}
export function tools_clipboardCopy(text) {
    try {
        navigator.clipboard.writeText(text).then(() => { });
        setTimeout(() => { }, 2000);
    } catch (error) {
        console.error(error);
        alert(error);
    }
}
export function tools_getUserAgent() {
    return navigator.userAgent;
}