using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SimpleServiceLibrary {
    public interface ISimpleService {
        string SimpleMethod();
        Task<ReadOnlyCollection<Uri>[]> GetWebsitesLinks(string[] domainNames);
    }
}
