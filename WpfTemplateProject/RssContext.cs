using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSSLoudReader.Models;

namespace RSSLoudReader
{
    class RssContext : DbContext
    {
        public DbSet<RssSource> RssSources { get; set; }
        public DbSet<RssEntry> RssEntries { get; set; }
    }
}
