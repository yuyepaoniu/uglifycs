using System;

namespace UglifyCS {
    public class Extensibility {
        private static Extensibility _Instance = new Extensibility();
        public static Extensibility Instance {
            get { return _Instance; }
            set { _Instance = value; }
        }

        private static string makePathSafe(string input) {
            foreach (var badchar in System.IO.Path.GetInvalidFileNameChars()) {
                input = input.Replace(badchar, '_');
            }
            return input;
        }

        private Uri _LocalStorage;
        private Uri LocalStorage {
            get {
                if (_LocalStorage == null)
                    _LocalStorage = new Uri(System.Environment.GetFolderPath(System.Environment.SpecialFolder.InternetCache));
                return _LocalStorage;
            }
            set {
                _LocalStorage = value;
            }
        }

        private static System.Net.HttpWebResponse GetResponse(System.Net.HttpWebRequest req) {
            try {
                return (System.Net.HttpWebResponse)req.GetResponse();
            } catch (System.Net.WebException ex) {
                return (System.Net.HttpWebResponse)ex.Response;
            }
        }

        public string GetContents(Uri uri, string file) {
            if (!file.EndsWith(".js", StringComparison.OrdinalIgnoreCase)) file += ".js";
            return GetContents(new Uri(uri, file));
        }

        public virtual void Download(Uri url, string file) {
            var exists = System.IO.File.Exists(file);
            var maxage = TimeSpan.FromHours(1);
            var date = !exists ? DateTime.MinValue : System.IO.File.GetLastWriteTimeUtc(file);
            if (!exists || date.Add(maxage) < DateTime.UtcNow) {
                var req = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                req.AllowAutoRedirect = true;
                req.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
                if (date > DateTime.MinValue) req.IfModifiedSince = date;

                using (var resp = GetResponse(req)) {
                    if (resp == null) return;
                    if (resp.StatusCode == System.Net.HttpStatusCode.OK) {
                        using (var stream = resp.GetResponseStream())
                        using (var filestream = new System.IO.FileStream(file, System.IO.FileMode.Create)) {
                            byte[] data = new byte[8096];
                            int read = 1;
                            while (read > 0) {
                                read = stream.Read(data, 0, data.Length);
                                filestream.Write(data, 0, read);
                            }
                        }

                    } else if (exists) {
                        if (resp.StatusCode == System.Net.HttpStatusCode.NotModified) {
                            //Don't check again for an hour
                            System.IO.File.SetLastWriteTime(file, DateTime.UtcNow.Add(maxage));

                        } else {
                            //something bad happened ... not going to try for 15 minutes
                            System.IO.File.SetLastWriteTime(file, DateTime.UtcNow.Subtract(maxage).AddMinutes(15));
                        }
                    }
                }
            }
        }

        public virtual string GetContents(Uri uri) {
            string file;
            if (uri.Scheme == "http" || uri.Scheme == "https") {
                file = new Uri(LocalStorage, makePathSafe(uri.ToString())).LocalPath;
                Download(uri, file);
            } else file = uri.LocalPath;

            if (!System.IO.File.Exists(file)) {
                if (file.EndsWith("env.js")) return UglifyCS.Properties.Resources.env;
                else if (file.EndsWith("uglify-js.js")) return UglifyCS.Properties.Resources.uglify_js;
                else if (file.EndsWith("uglifyjs.js")) return UglifyCS.Properties.Resources.uglifyjs;
                else if (file.EndsWith("squeeze-more.js")) return UglifyCS.Properties.Resources.squeeze_more;
                else if (file.EndsWith("parse-js.js")) return UglifyCS.Properties.Resources.parse_js;
                else if (file.EndsWith("process.js")) return UglifyCS.Properties.Resources.process;
                else if (file.EndsWith("beautify.js")) return UglifyCS.Properties.Resources.beautify;
                else if (file.EndsWith("jshint.js")) return UglifyCS.Properties.Resources.jshint;
                else if (file.EndsWith("jsUnitCore.js")) return UglifyCS.Properties.Resources.jsUnitCore;
                else if (file.EndsWith("browser.js")) return UglifyCS.Properties.Resources.browser;
                else if (file.EndsWith("coffee-script.js")) return UglifyCS.Properties.Resources.coffee_script;
                else if (file.EndsWith("csslint.js")) return UglifyCS.Properties.Resources.csslint;
            }

            return System.IO.File.ReadAllText(file);
        }


        public virtual Uri GetBaseLocation(Environment environment) {
            if (environment is JSHint) return new Uri("http://jshint.com/jshint.js");
            if (environment is CoffeeScript) return new Uri("http://jashkenas.github.com/coffee-script/extras/coffe-script.js");
            if (environment is Beautify) return new Uri("http://jsbeautifier.org/beautify.js");
            if (environment is CSSLint) return new Uri("http://csslint.net/js/csslint.js");
            return null;
        }
    }
}
