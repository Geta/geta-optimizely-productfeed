// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Web;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Models;
using Geta.Optimizely.ProductFeed.Repositories;
using Xunit;

namespace Geta.Optimizely.ProductFeed.Tests;

public class ProcessingPipelineTests
{
    private readonly List<string> _statuses = [];
    private readonly RecordingRepository _repository = new();
    private readonly RecordingExporter _exporter = new();

    [Fact]
    public void Process_ExportsMappedFilteredEntitiesForEveryHost()
    {
        var pipeline = CreatePipeline(
            codes: ["1", "2", "skip", null],
            filter: new CodeFilter(c => c != "skip"),
            hosts: [new HostDefinition { Name = "a.com" }, new HostDefinition { Name = "b.com" }]);

        pipeline.Process(new JobStatusLogger(_statuses.Add), CancellationToken.None);

        Assert.Equal(["1", "2", "1", "2"], _exporter.Built);
        Assert.Equal(2, _repository.Saved.Count);
        Assert.True(_exporter.Disposed);
    }

    [Fact]
    public void Process_ReportsProgress()
    {
        var pipeline = CreatePipeline(codes: Enumerable.Range(1, 40).Select(i => i.ToString()).ToArray());

        pipeline.Process(new JobStatusLogger(_statuses.Add), CancellationToken.None);

        Assert.Contains(_statuses, s => s.EndsWith("Loaded 40 entities."));
        Assert.Contains("Enriching entities: 20 of 40 (50%)", _statuses);
        Assert.Contains("Enriching entities: 40 of 40 (100%)", _statuses);
        Assert.Contains(_statuses, s => s.StartsWith("Building feeds for ") && s.EndsWith("40 of 40 (100%)"));
    }

    [Fact]
    public void Process_WhenCancelled_StopsAndDisposesExporters()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pipeline = CreatePipeline(codes: ["1"]);

        Assert.Throws<OperationCanceledException>(() => pipeline.Process(new JobStatusLogger(_statuses.Add), cts.Token));
        Assert.Empty(_exporter.Built);
        Assert.Empty(_repository.Saved);
        Assert.True(_exporter.Disposed);
    }

    private ProcessingPipeline<TestEntity> CreatePipeline(
        string[] codes,
        IProductFeedFilter<TestEntity> filter = null,
        HostDefinition[] hosts = null)
    {
        var descriptor = new FeedDescriptor("test", "/feed", "text/plain");

        return new ProcessingPipeline<TestEntity>(
            new FixedSiteBuilder(hosts ?? [new HostDefinition { Name = "example.com" }]),
            new FixedContentLoader(codes.Length),
            new IndexMapper(codes),
            [new NoOpEnricher()],
            [descriptor],
            _ => _exporter,
            _repository,
            filter);
    }

    public class TestEntity
    {
        public string Code { get; set; }
    }

    private class FixedSiteBuilder(HostDefinition[] hosts) : ISiteBuilder
    {
        public IEnumerable<HostDefinition> GetHosts() => hosts;
    }

    private class FixedContentLoader(int count) : IProductFeedContentLoader
    {
        public IEnumerable<CatalogContentBase> LoadSourceData(CancellationToken cancellationToken) =>
            Enumerable.Range(0, count).Select(i => new TestProduct { Code = i.ToString() });
    }

    // Maps the n-th loaded item to codes[n]; a null code maps to a null entity.
    private class IndexMapper(string[] codes) : IEntityMapper<TestEntity>
    {
        public TestEntity Map(CatalogContentBase catalogContent)
        {
            var code = codes[int.Parse(((TestProduct)catalogContent).Code)];
            return code == null ? null : new TestEntity { Code = code };
        }
    }

    private class CodeFilter(Func<string, bool> include) : IProductFeedFilter<TestEntity>
    {
        public bool ShouldInclude(TestEntity entity) => include(entity.Code);
    }

    private class NoOpEnricher : IProductFeedContentEnricher<TestEntity>
    {
        public TestEntity Enrich(TestEntity sourceData, CancellationToken cancellationToken) => sourceData;
    }

    private class RecordingExporter : AbstractFeedContentExporter<TestEntity>, IDisposable
    {
        public List<string> Built { get; } = [];

        public bool Disposed { get; private set; }

        public override void BuildEntry(TestEntity entity, HostDefinition host, CancellationToken cancellationToken) =>
            Built.Add(entity.Code);

        public override ICollection<FeedEntity> FinishExport(HostDefinition host, CancellationToken cancellationToken) =>
            [new FeedEntity { Link = host.Name }];

        public override byte[] SerializeEntry(object value, CancellationToken cancellationToken) => [];

        public override object ConvertEntry(TestEntity entity, HostDefinition host, CancellationToken cancellationToken) => entity;

        public void Dispose() => Disposed = true;
    }

    private class RecordingRepository : IFeedRepository
    {
        public List<FeedEntity> Saved { get; } = [];

        public FeedEntity GetLatestFeed(Uri siteUri) => null;

        public void Save(ICollection<FeedEntity> feedData) => Saved.AddRange(feedData);

        public Task SaveAsync(ICollection<FeedEntity> feedData, CancellationToken cancellationToken)
        {
            Save(feedData);
            return Task.CompletedTask;
        }

        public FeedDescriptor FindDescriptorByUri(Uri siteUri) => null;
    }
}
