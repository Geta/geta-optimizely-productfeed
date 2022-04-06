// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;

namespace Geta.Optimizely.ProductFeed.Models
{
    public interface IFeed
    {
        DateTime Updated { get; }

        string Link { get; }
    }
}
