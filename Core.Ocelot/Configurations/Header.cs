using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Ocelot.Configurations
{
    public class Header
    {
        public Header(string key, IEnumerable<string> values)
        {
            Key = key;
            Values = values ?? new List<string>();
        }

        public string Key { get; }
        public IEnumerable<string> Values { get; }
    }
}
