﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RSSLoudReader.Models;

namespace RSSLoudReader.Events
{
    class RssFeedAdded
    {
        public RssFeedAdded(RssEntry rssEntry)
        {
            RssEntry = rssEntry;
        }

        public RssEntry RssEntry { get; set; }
    }
}
