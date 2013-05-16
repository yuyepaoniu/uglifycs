//#! /usr/bin/env node
// -*- js2 -*-

var uglify = require("uglify-js"), // symlink ~/.node_libraries/uglify-js.js to ../uglify-js.js
    jsp = uglify.parser,
    pro = uglify.uglify;

pro.set_logger(function(msg){
        sys.debug(msg);
});

var options = {
        ast: false,
        mangle: true,
        mangle_toplevel: false,
        squeeze: true,
        make_seqs: true,
        dead_code: true,
        beautify: false,
        verbose: false,
        show_copyright: true,
        out_same_file: false,
        max_line_length: 32 * 1024,
        extra: false,
        unsafe: false,            // XXX: extra & unsafe?  but maybe we don't want both, so....
        beautify_options: {
                indent_level: 4,
                indent_start: 0,
                quote_keys: false,
                space_colon: false
        },
        output: true            // stdout
};

// --------- main ends here.

function show_copyright(comments) {
        var ret = "";
        for (var i = 0; i < comments.length; ++i) {
                var c = comments[i];
                if (c.type == "comment1") {
                        ret += "//" + c.value + "\n";
                } else {
                        ret += "/*" + c.value + "*/";
                }
        }
        return ret;
};

function squeeze_it(code, options) {
        var result = "";
        if (options.show_copyright) {
                var initial_comments = [];
                // keep first comment
                var tok = jsp.tokenizer(code, false), c;
                c = tok();
                var prev = null;
                while (/^comment/.test(c.type) && (!prev || prev == c.type)) {
                        initial_comments.push(c);
                        prev = c.type;
                        c = tok();
                }
                result += show_copyright(initial_comments);
        }
        try {
                var ast = time_it("parse", function(){ return jsp.parse(code); });
                if (options.mangle)
                        ast = time_it("mangle", function(){ return pro.ast_mangle(ast, options.mangle_toplevel); });
                if (options.squeeze)
                        ast = time_it("squeeze", function(){
                                ast = pro.ast_squeeze(ast, {
                                        make_seqs : options.make_seqs,
                                        dead_code : options.dead_code,
                                        extra     : options.extra
                                });
                                if (options.unsafe)
                                        ast = pro.ast_squeeze_more(ast);
                                return ast;
                        });
                if (options.ast)
                        return sys.inspect(ast, null, null);
                result += time_it("generate", function(){ return pro.gen_code(ast, options.beautify && options.beautify_options) });
                if (!options.beautify && options.max_line_length) {
                        result = time_it("split", function(){ return pro.split_lines(result, options.max_line_length) });
                }
                return result;
        } catch(ex) {
                sys.debug(ex.stack);
                sys.debug(sys.inspect(ex));
                sys.debug(JSON.stringify(ex));
        }
};

function time_it(name, cont) {
        if (!options.verbose)
                return cont();
        var t1 = new Date().getTime();
        try { return cont(); }
        finally { sys.debug("// " + name + ": " + ((new Date().getTime() - t1) / 1000).toFixed(3) + " sec."); }
};