var window = {},  require = (function () {
    var exported = {};
    var files = {};

    return function (file) {
        file = getFullFilename(file);
        var key = file.toLowerCase();

        if (files[key]) {
            return exported[key];
        }

        var js = getContents(file);
        var func = new Function('var exports = {}; ' + js + '; return exports;');

        files[key] = true;
        return exported[key] = func();
    }
})();