// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using Geta.Optimizely.GoogleProductFeed.Models;

namespace Geta.Optimizely.GoogleProductFeed
{
    public abstract class FeedBuilder
    {
        public abstract List<Feed> Build();
    }
}
