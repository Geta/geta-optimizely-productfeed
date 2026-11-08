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

    [Fact]
    public void Process_WhenStoppedWhileLoading_StopsMapping()
    {
        using var cts = new CancellationTokenSource();
        var mapper = new CancellingMapper(cts, cancelOnCall: 2);
        var pipeline = CreatePipeline(codes: ["1", "2", "3", "4"], mapper: mapper);

        Assert.Throws<OperationCanceledException>(() => pipeline.Process(new JobStatusLogger(_statuses.Add), cts.Token));
        Assert.Equal(2, mapper.Calls);
        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public void Process_WhenStoppedWithNothingLoaded_KeepsStoredFeeds()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var pipeline = CreatePipeline(codes: []);

        Assert.Throws<OperationCanceledException>(() => pipeline.Process(new JobStatusLogger(_statuses.Add), cts.Token));
        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public void Process_WhenDisposeFails_KeepsTheExceptionThatEndedTheExport()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var logger = new JobStatusLogger(_statuses.Add);
        var pipeline = CreatePipeline(codes: ["1"], exporter: new ThrowingDisposeExporter());

        Assert.Throws<OperationCanceledException>(() => pipeline.Process(logger, cts.Token));
        Assert.Contains("Failed to dispose exporter", logger.ToString());
    }

    [Fact]
    public void Process_ExportsEntitiesReturnedByEnrichers()
    {
        var pipeline = CreatePipeline(codes: ["1", "2"], enricher: new ReplacingEnricher());

        pipeline.Process(new JobStatusLogger(_statuses.Add), CancellationToken.None);

        Assert.Equal(["1 enriched", "2 enriched"], _exporter.Built);
    }

    private ProcessingPipeline<TestEntity> CreatePipeline(
        string[] codes,
        IProductFeedFilter<TestEntity>? filter = null,
        HostDefinition[]? hosts = null,
        IEntityMapper<TestEntity>? mapper = null,
        IProductFeedContentEnricher<TestEntity>? enricher = null,
        AbstractFeedContentExporter<TestEntity>? exporter = null)
    {
        var descriptor = new FeedDescriptor("test", "/feed", "text/plain");

        return new ProcessingPipeline<TestEntity>(
            new FixedSiteBuilder(hosts ?? [new HostDefinition { Name = "example.com" }]),
            new FixedContentLoader(codes.Length),
            mapper ?? new IndexMapper(codes),
            [enricher ?? new NoOpEnricher()],
            [descriptor],
            _ => exporter ?? _exporter,
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

    // Cancels the job's token while mapping the n-th item, like a Stop clicked during "Loading catalog content...".
    private class CancellingMapper(CancellationTokenSource cts, int cancelOnCall) : IEntityMapper<TestEntity>
    {
        public int Calls { get; private set; }

        public TestEntity Map(CatalogContentBase catalogContent)
        {
            if (++Calls == cancelOnCall)
            {
                cts.Cancel();
            }

            return new TestEntity { Code = ((TestProduct)catalogContent).Code };
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

    // Returns a new instance instead of changing the one it is given.
    private class ReplacingEnricher : IProductFeedContentEnricher<TestEntity>
    {
        public TestEntity Enrich(TestEntity sourceData, CancellationToken cancellationToken) =>
            new() { Code = $"{sourceData.Code} enriched" };
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

    private class ThrowingDisposeExporter : RecordingExporter, IDisposable
    {
        void IDisposable.Dispose() => throw new InvalidOperationException("Dispose failed");
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
