using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IRC.Models
{
    public class Message
    {
        public string RawMessage { get; set; }

        //Only use prefix as registered nickname of self, not required
        public string? Prefix { get; set; }
        public string Command { get; set; }
        public List<string> Params { get; set; }
        public string? Trailing { get; set; }
    }
}
