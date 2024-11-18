// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using EPiServer.Web;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed;

public abstract class AbstractFeedContentExporter<TEntity>
{
    protected MemoryStream _buffer = new();

    public IProductFeedConverter<TEntity> Converter { get; private set; }

    public IProductFeedFilter<TEntity> Filter { get; private set; }

    public ISiteUrlBuilder SiteUrlBuilder { get; private set; }

    public FeedDescriptor Descriptor { get; private set; }

    public virtual void BeginExport(HostDefinition host, CancellationToken cancellationToken)
    {
        _buffer = new();
    }

    public virtual void BuildEntry(TEntity entity, HostDefinition host, CancellationToken cancellationToken)
    {
        var shouldInclude = Filter?.ShouldInclude(entity) ?? true;

        if (!shouldInclude)
        {
            return;
        }

        var entry = ConvertEntry(entity, host, cancellationToken);
        if (entry == null)
        {
            return;
        }

        var serialized = SerializeEntry(entry, cancellationToken);
        if (serialized != null)
        {
            _buffer.Write(serialized);
        }
    }

    public abstract byte[] SerializeEntry(object value, CancellationToken cancellationToken);

    public abstract object ConvertEntry(TEntity entity, HostDefinition host, CancellationToken cancellationToken);

    public virtual ICollection<FeedEntity> FinishExport(HostDefinition host, CancellationToken cancellationToken)
    {
        return new[]
        {
            new FeedEntity
            {
                CreatedUtc = DateTime.UtcNow,
                Link = $"{host.Url.ToString().TrimEnd('/')}/{Descriptor.FileName.TrimStart('/')}",
                FeedBytes = _buffer.ToArray()
            }
        };
    }

    public void SetConverter(IProductFeedConverter<TEntity> converter)
    {
        Converter = converter;
    }

    public void SetFilter(IProductFeedFilter<TEntity> filter)
    {
        Filter = filter;
    }

    public void SetSiteUrlBuilder(ISiteUrlBuilder urlBuilder)
    {
        SiteUrlBuilder = urlBuilder;
    }

    public void SetDescriptor(FeedDescriptor feedDescriptor)
    {
        Descriptor = feedDescriptor;
    }
}
