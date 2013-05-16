using System;
using System.Collections;
using System.Text;

namespace UglifyCS {
    internal class SystemConsole {
        public void log(object param1) {
            log(new[] { param1 });
        }
        public void log(object param1, object param2) {
            log(new[] { param1, param2 });
        }
        public void log(object param1, object param2, object param3) {
            log(new[] { param1, param2, param3 });
        }
        private void log(params object[] parameters) {
            StringBuilder line = null;
            foreach (var param in parameters) {
                if (line == null) line = new StringBuilder();
                else line.Append('\t');


                if (param is IDictionary) {
                    var def = "";
                    var dic = (IDictionary)param;
                    foreach (var subparam in dic.Keys) {
                        def += ", " + Convert.ToString(subparam) + ": " + Convert.ToString(dic[subparam]);
                    }
                    line.Append("{ " + def.Substring(2) + " }");
                    continue;

                } else if (param is IEnumerable && !(param is string)) {
                    var def = "";
                    foreach (var subparam in (IEnumerable)param) {
                        def += ", " + Convert.ToString(subparam);
                    }
                    line.Append("[ " + def.Substring(2) + " ]");
                    continue;
                }
                line.Append(param);
            }

            Console.WriteLine(line == null ? null : line.ToString());

        }
    }
}
