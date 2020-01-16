using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleServiceLibrary
{
    public class HrefLinkParserCore : IParserLinkCore
    {
        private readonly Regex regex;

        protected virtual string RegexAnchorPattern => "<a\\s+(?:[^>]*?\\s+)?href=([\"'])(.*?)\\1";

        public HrefLinkParserCore()
        {
            this.regex = new Regex(RegexAnchorPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public ReadOnlyCollection<string> ProcessPage(string page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            return DoProcessPage(page);
        }

        protected virtual ReadOnlyCollection<string> DoProcessPage(string page)
        {
            var uriList = new List<string>();
            var match = this.regex.Match(page);
            while (match.Success)
            {
                string url = match.Groups[2].Captures[0].ToString();
                if (!url.Contains("#"))
                    uriList.Add(url);
                match = match.NextMatch();
            }

            return new ReadOnlyCollection<string>(uriList);
        }
    }
}
