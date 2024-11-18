// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using Mediachase.Commerce.Catalog;

namespace Geta.Optimizely.ProductFeed;

public class DefaultProductFeedContentLoader(
    IContentLoader contentLoader,
    ReferenceConverter referenceConverter,
    IContentLanguageAccessor languageAccessor)
    : IProductFeedContentLoader
{
    public IEnumerable<CatalogContentBase> LoadSourceData(CancellationToken cancellationToken)
    {
        var catalogReferences = contentLoader.GetDescendents(referenceConverter.GetRootLink());
        var items = contentLoader.GetItems(catalogReferences, CreateDefaultLoadOption()).OfType<CatalogContentBase>();

        return items;
    }

    private LoaderOptions CreateDefaultLoadOption()
    {
        var loaderOptions = new LoaderOptions { LanguageLoaderOption.FallbackWithMaster(languageAccessor.Language) };

        return loaderOptions;
    }
}
