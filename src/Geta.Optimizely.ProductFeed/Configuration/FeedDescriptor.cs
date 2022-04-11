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

        //public void SetExporter<TExporter>() where TExporter : AbstractFeedContentExporter<TEntity>
        //{
        //    Exporter = typeof(TExporter);
        //}

        //public void SetConverter<TConverter>() where TConverter : IProductFeedConverter<TEntity>
        //{
        //    Converter = typeof(TConverter);
        //}

        public void SetSiteUrlBuilder<TBuilder>() where TBuilder : ISiteUrlBuilder
        {
            SiteUrlBuilder = typeof(TBuilder);
        }
    }

    public static class FeedDescriptorExtensions
    {
        public static FeedDescriptor SetExporter<TExporter, TEntity>(this FeedDescriptor descriptor)
            where TExporter : AbstractFeedContentExporter<TEntity>
        {
            descriptor.Exporter = typeof(TExporter);
            return descriptor;
        }

        //public static FeedDescriptor SetConverter<TConverter, TEntity>(this FeedDescriptor descriptor)
        //    where TConverter : IProductFeedConverter<TEntity>
        //{
        //    descriptor.Converter = typeof(TConverter);
        //    return descriptor;
        //}
    }
}
