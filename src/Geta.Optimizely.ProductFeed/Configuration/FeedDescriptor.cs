// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;

namespace Geta.Optimizely.ProductFeed.Configuration
{
    public class FeedDescriptor
    {
        public FeedDescriptor(string name, string fileName, string mimeType)
        {
            Name = name;
            FileName = fileName;
            MimeType = mimeType;
        }

        public string Name { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public Type Exporter { get; set; }

        public Type Converter { get; set; }

        public Type SiteUrlBuilder { get; set; }

        public void SetSiteUrlBuilder<TBuilder>() where TBuilder : ISiteUrlBuilder
        {
            SiteUrlBuilder = typeof(TBuilder);
        }
    }
}
