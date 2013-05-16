
using System;
namespace UglifyCS {
    public class JSUnit : Environment {
        public class AssertFailedException : Exception {
            public AssertFailedException(string message, string condition, object result)
                : base(message) {
                Condition = condition;
            }
            public string Condition { get; private set; }
            public object Result { get; private set; }
        }

        private string[] _dependencies;
        public JSUnit(params string[] dependencies) {
            _dependencies = dependencies;
        }

        protected override void OnInit() {
            foreach (var file in _dependencies)
                RunFile(file);
        }

        public void Assert(string condition) {
            condition = string.IsNullOrEmpty(condition) ? "undefined" : condition;

            var script = "var result = (function(){ return " + condition.Trim() + "; })(); var pass = result ? true : false;";
            Run(script);

            var pass = (bool)this["pass"];
            if (!pass) {
                var result = this["result"];
                var message = string.Format("Assertion failed with {0} ({1})", result ?? "null", condition);
                throw new AssertFailedException(message, condition, result);
            }
        }
    }
}
