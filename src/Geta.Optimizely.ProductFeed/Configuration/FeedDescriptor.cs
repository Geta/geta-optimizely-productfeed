// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;

namespace Geta.Optimizely.ProductFeed.Configuration
{
    public abstract class FeedDescriptor
    {
        protected FeedDescriptor(string name, string fileName, string mimeType)
        {
            Name = name;
            FileName = fileName;
            MimeType = mimeType;
        }

        public string Name { get; }

        public string FileName { get; }

        public string MimeType { get; }

        public Type Exporter { get; private set; }

        public Type Converter { get; protected set; }

        public void SetExporter<TExporter>() where TExporter : IProductFeedContentExporter
        {
            Exporter = typeof(TExporter);
        }
    }
}
