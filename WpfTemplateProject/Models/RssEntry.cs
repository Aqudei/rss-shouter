using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSLoudReader.Models
{
    public class RssEntry
    {
        public int Id { get; set; }
        public string GeneratedId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        //public string Source { get; set; }
    }
}
