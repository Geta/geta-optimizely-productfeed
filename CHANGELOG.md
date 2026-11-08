# Changelog

All notable changes to this project will be documented in this file.

## [5.1.0]

* CSV export can write several rows for one entity: return a collection of CSV records from the converter (#20)
* Fixed feeds not being found for internationalized domain names, e.g. `bücher.de` (#14)
* Feed links are now stored with the IDN-encoded host; existing rows are still found and are rewritten the next time the job runs
* The `ProductFeed - Create feeds` job now reports progress in its status while enriching entities and building feeds (#11)
* Stopping the job now takes effect between entities, also while catalog content is loaded, and is logged as a stop instead of an error
* Serving a feed no longer loads the content of every stored feed into memory, and saving a feed no longer reads the previous version first (#13)
* Entities the mapper returns as `null` are skipped before the entity filter runs
* The entity an enricher returns is now used, so an enricher can return a new instance (for example a record updated with `with`)
* Fixed feeds whose file name contains non-ASCII characters or spaces not being served
* Job log timestamps use a 24-hour clock and the invariant culture
* Added unit tests (#7)

## [5.0.0]

* Target .NET 10
* Updated to Optimizely CMS 13 / Commerce 15 (`EPiServer.Commerce.Core` 15.0.0)
* Updated Entity Framework Core to 10.0.5

## [4.0.1]
* Fixed issue with CsvWriter initialization

## [4.0.0]

* Target .NET8
* Package upgrades

## [4.0.0]

* Added command execution timeout configuration option for EF Core DbContext

## [3.0.1]

* Fixed issue with internationalized domain name (https://github.com/Geta/geta-optimizely-productfeed/issues/14)

## [3.0.0]

* Target .NET 6.0
* Upgrade Commerce version to v14.12.0

## [2.3.0]

* Fixed CSV exporter (wrong destination buffer was used at the beginning of the export)

## [2.2.2]

* Added mult-site / multi-language support
* Fix for supporting multi-site feed serving procedure

## [2.1.0]

* Option to specify entity filter
* Added option to specify filter for each feed exporter

## [2.0.0]

* Extracted Google feed code into a separate package (`Geta.Optimizely.ProductFeed.Google`)
* Renamed package to `Geta.Optimizely.ProductFeed`
* Added CSV export package

## [1.0.0]

Initial Optimizely version of the package.