using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleServiceLibrary
{
    public class AbsoluteLinkParserCore : IParserLinkCore
    {
        private readonly Regex regex;

        protected virtual string RegexAnchorPattern => @"((https?://)|(www\d?\.))[a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";

        public AbsoluteLinkParserCore()
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
                var url = match.ToString();
                uriList.Add(url);
                match = match.NextMatch();
            }

            return new ReadOnlyCollection<string>(uriList);
        }
    }
}
