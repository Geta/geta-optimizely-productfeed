# Changelog

All notable changes to this project will be documented in this file.

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