// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.IO;
using System.Text;

namespace Geta.Optimizely.ProductFeed.Google;

public sealed class EncodedStringWriter(Encoding encoding) : StringWriter
{
    public override Encoding Encoding { get; } = encoding;
}
