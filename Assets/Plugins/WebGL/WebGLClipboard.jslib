// WebGLClipboard.jslib
mergeInto(LibraryManager.library, {
    CopyToClipboard: function(textPointer) {
        var text = UTF8ToString(textPointer);
        navigator.clipboard.writeText(text).then(function() {
            console.log('Text copied to clipboard');
        }).catch(function(err) {
            console.error('Could not copy text:', err);
        });
    }
});