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

        public Type Exporter { get; private set; }

        public Type Converter { get; protected set; }

        public Type SiteUrlBuilder { get; protected set; }

        public void SetExporter<TExporter>() where TExporter : AbstractFeedContentExporter
        {
            Exporter = typeof(TExporter);
        }

        public void SetConverter<TConverter>() where TConverter : IProductFeedConverter
        {
            Converter = typeof(TConverter);
        }

        public void SetSiteUrlBuilder<TBuilder>() where TBuilder : ISiteUrlBuilder
        {
            SiteUrlBuilder = typeof(TBuilder);
        }
    }
}
