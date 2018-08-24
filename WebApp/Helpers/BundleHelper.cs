using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace apps.vaicys.com.Helpers
{
    public class BundleHelper
    {
        private readonly IHostingEnvironment _env;
        private IDictionary<string, IList<string>> _bundles;
        private IDictionary<string, string> _hashes;

        private const string bundleFile = "bundleconfig.json";
        private const string cssTemplate = "<link rel=\"stylesheet\" href=\"{0}?{1}\" />";
        private const string jsTempalte = "<script src=\"{0}?{1}\"></script>";
        private const string wwwRootPrefix = "wwwroot";

        private class BundleItem
        {
            public string OutputFileName { get; set; }
            public IList<string> InputFiles { get; set; }
        }

        public BundleHelper(IHostingEnvironment env)
        {
            _env = env;
        }

        public IHtmlContent RenderBumdle(string bundleName)
        {

            if (_bundles == null)
            {
                LoadBundleInformation();
            }

            if (bundleName.StartsWith("~")) bundleName = bundleName.Substring(1);
            if (!bundleName.StartsWith("/")) bundleName = "/" + bundleName;
            if (!bundleName.StartsWith(wwwRootPrefix)) bundleName = wwwRootPrefix + bundleName;

            if (_env.IsProduction())
            {
                return new HtmlString(Format(bundleName, _hashes[bundleName]));
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append(string.Join(Environment.NewLine, _bundles[bundleName].Select(file => Format(file, GetSha1Hash(file)))));
                return new HtmlString(sb.ToString());
            }
        }

        private string Format(string input, string hash)
        {
            var noPrefix = input.Replace(wwwRootPrefix, string.Empty);
            if (noPrefix.EndsWith("css"))
            {
                return string.Format(cssTemplate, noPrefix, hash);
            }
            return string.Format(jsTempalte, noPrefix, hash);
        }

        private void LoadBundleInformation()
        {
            _bundles = new Dictionary<string, IList<string>>();
            _hashes = new Dictionary<string, string>();

            if (!File.Exists(bundleFile))
            {
                return;
            }

            var items = JsonConvert.DeserializeObject<IList<BundleItem>>(File.ReadAllText(bundleFile));

            foreach (var item in items)
            {
                _bundles.Add(
                    item.OutputFileName,
                    item.InputFiles.ToList());

                _hashes.Add(item.OutputFileName, GetSha1Hash(item.OutputFileName));
            }
        }

        private string GetSha1Hash(string fileName)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                var hash = sha1.ComputeHash(File.ReadAllBytes(fileName));
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            }
        }
    }
}
