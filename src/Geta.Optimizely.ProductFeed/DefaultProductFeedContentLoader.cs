// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using Mediachase.Commerce.Catalog;

namespace Geta.Optimizely.ProductFeed
{
    public class DefaultProductFeedContentLoader : IProductFeedContentLoader
    {
        private readonly IContentLoader _contentLoader;
        private readonly IContentLanguageAccessor _languageAccessor;
        private readonly ReferenceConverter _referenceConverter;

        public DefaultProductFeedContentLoader(
            IContentLoader contentLoader,
            ReferenceConverter referenceConverter,
            IContentLanguageAccessor languageAccessor)
        {
            _contentLoader = contentLoader;
            _referenceConverter = referenceConverter;
            _languageAccessor = languageAccessor;
        }

        public ICollection<CatalogContentBase> LoadSourceData()
        {
            var catalogReferences = _contentLoader.GetDescendents(_referenceConverter.GetRootLink());
            var items = _contentLoader.GetItems(catalogReferences, CreateDefaultLoadOption()).OfType<CatalogContentBase>();

            return items.ToList();
        }

        private LoaderOptions CreateDefaultLoadOption()
        {
            var loaderOptions = new LoaderOptions { LanguageLoaderOption.FallbackWithMaster(_languageAccessor.Language) };

            return loaderOptions;
        }
    }
}
