// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using Geta.Optimizely.ProductFeed.Configuration;

namespace Geta.Optimizely.ProductFeed.Models
{
    public class FeedMedia
    {
        public FeedMedia(byte[] data, FeedDescriptor descriptor)
        {
            Data = data;
            Descriptor = descriptor;
        }

        public byte[] Data { get; }

        public FeedDescriptor Descriptor { get; }
    }
}
