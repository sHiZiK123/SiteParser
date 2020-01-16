using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceLibrary
{
    public interface ISiteParser
    {
        Task<ReadOnlyCollection<System.Uri>> GetUrlTreeAsync(IEnumerable<IParserLinkCore> parsingStrategies, System.Uri address);
    }
}
