using System;
using System.Data;
using NugetGallery;

namespace NuGetGallery
{
    public class AggregateStatsService : IAggregateStatsService
    {
        public AggregateStats GetAggregateStats()
        {
            return Cache.Get("aggregatestats",
                    DateTime.UtcNow.AddMinutes(5),
                    () =>
                    {
                        using (var dbContext = new EntitiesContext())
                        {
                            var database = dbContext.Database;
                            using (var command = database.Connection.CreateCommand())
                            {
                                command.CommandText = @"SELECT 
  (Select COUNT([Key]) From PackageRegistrations pr Where Exists (select 1 from Packages p where p.PackageRegistrationKey = pr.[Key] and p.Listed = 1)) AS UniquePackages,
  (Select COUNT([Key]) From Packages Where Listed = 1) AS TotalPackages,
  (Select SUM(DownloadCount) From Packages) AS DownloadCount,
  (Select COUNT([Key]) From [dbo].[Packages] Where [Status] = 'Submitted' And SubmittedStatus <> 'Waiting') AS PackagesReadyForReview,
  (Select COUNT([Key]) From [dbo].[Packages] Where [Status] = 'Submitted') AS AllPackagesUnderModeration,
  (Select AVG(DATEDIFF(HOUR, Created, ApprovedDate)) From dbo.Packages Where [Status] = 'Approved' And ReviewedById is Not Null And Created >= DATEADD(DAY, -30, GETUTCDATE())) AS AverageModerationWaitTime,
  (Select COUNT([Key]) From [dbo].[Packages] Where IsLatest = 1 And Created >= DATEADD(MONTH, -4, GETUTCDATE())) AS UpToDatePackages,
  (Select COUNT([Key]) From [dbo].[Packages] Where IsLatest = 1 And Created < DATEADD(YEAR, -1, GETUTCDATE())) AS OlderThanOneYearPackages,
  (Select COUNT([Key]) From [dbo].[Packages] Where [Status] = 'Approved' And [IsLatestStable] = 1) AS ApprovedPackages,
  (Select COUNT([Key]) From [dbo].[Packages] Where [Status] = 'Approved') AS TotalApprovedPackages,
  (Select COUNT([Key]) From [dbo].[Packages] Where [Status] = 'Approved' And [ReviewedById] Is Not Null And [IsLatestStable] = 1) AS ManuallyApprovedPackages,
  (Select COUNT([Key]) From [dbo].[Packages] Where [Status] = 'Approved' And [ReviewedById] Is Not Null) AS TotalManuallyApprovedPackages,
  (Select COUNT([Key]) From [dbo].[Packages] Where [Status] = 'Approved' And [ReviewedById] Is Null And [IsLatestStable] = 1) AS TrustedPackages,
  (Select COUNT([Key]) From [dbo].[Packages] Where [Status] = 'Approved' And [ReviewedById] Is Null) AS TotalTrustedPackages,
  (Select Count([Key]) From [dbo].[Packages] Where [Status] = 'Rejected') AS TotalRejectedPackages,
  (Select COUNT([Key]) From [dbo].[Packages] Where [Status] = 'Exempted' And [ReviewedById] Is Not Null And [IsLatestStable] = 1) AS ExemptedPackages,
  (Select COUNT([Key]) From [dbo].[Packages] Where [Status] = 'Exempted' And [ReviewedById] Is Not Null) AS TotalExemptedPackages,
  (Select COUNT([Key]) From [dbo].[Packages] Where ([Status] <> 'Approved' Or [Status] Is Null) And [IsLatestStable] = 1) AS UnknownPackages,
  (Select COUNT([Key]) From [dbo].[Packages] Where ([Status] <> 'Approved' Or [Status] Is Null)) AS TotalUnknownPackages,
  (Select COUNT([Key]) From [dbo].[Packages] Where [IsLatest] = 1 And [IsLatestStable] = 0) AS LatestPackagePrerelease,
  (Select COUNT([Key]) From [dbo].[Packages] Where [Listed] = 0) AS TotalUnlistedPackages,
  (Select COUNT([Key]) From [dbo].[Packages] Where [IsLatestStable] = 1 And [PackageSourceUrl] Is Not Null) AS PackagesWithPackageSource,
  (Select COUNT([Key]) From [dbo].[Packages] Where IsLatestStable = 1 And PackageTestResultStatus = 'Passing') AS PackagesPassingVerification,
  (Select COUNT([Key]) From [dbo].[Packages] Where IsLatestStable = 1 And PackageTestResultStatus = 'Failing') AS PackagesFailingVerification,
  (Select COUNT([Key]) From [dbo].[Packages] Where [IsLatestStable] = 1 And PackageValidationResultStatus = 'Passing') AS PackagesPassingValidation,
  (Select COUNT([Key]) From [dbo].[Packages] Where [IsLatestStable] = 1 And PackageValidationResultStatus = 'Failing') AS PackagesFailingValidation,
  (Select COUNT([Key]) From [dbo].[Packages] Where [IsLatestStable] = 1 And DownloadCacheDate Is Not Null) AS PackagesCached,
  (Select COUNT([Key]) From [dbo].[Packages] Where DownloadCacheStatus = 'Available' And DownloadCacheDate Is Not Null) AS TotalPackagesCached,
  (Select COUNT([Key]) From [dbo].[Packages] Where [IsLatestStable] = 1 And DownloadCacheStatus = 'Available') AS PackagesCachedAvailable,
  (Select COUNT([Key]) From [dbo].[Packages] Where DownloadCacheStatus = 'Available') AS TotalPackagesCachedAvailable,
  (Select COUNT([Key]) From [dbo].[Packages] Where [IsLatestStable] = 1 And DownloadCacheStatus = 'Investigate') AS PackagesCachedInvestigate,
  (Select COUNT([Key]) From [dbo].[Packages] Where DownloadCacheStatus = 'Investigate') AS TotalPackagesCachedInvestigate,
  (Select COUNT([Key]) From [dbo].[Packages] Where [IsLatestStable] = 1 And PackageScanResultDate Is Not Null) AS PackagesScanned,
  (Select COUNT([Key]) From [dbo].[Packages] Where PackageScanResultDate Is Not Null) AS TotalPackagesScanned,
  (Select COUNT([Key]) From [dbo].[Packages] Where [IsLatestStable] = 1 And PackageScanStatus = 'NotFlagged') AS PackagesScannedNotFlagged,
  (Select COUNT([Key]) From [dbo].[Packages] Where PackageScanStatus = 'NotFlagged') AS TotalPackagesScannedNotFlagged,
  (Select COUNT([Key]) From [dbo].[Packages] Where [IsLatestStable] = 1 And PackageScanStatus = 'Flagged') AS PackagesScannedFlagged,
  (Select COUNT([Key]) From [dbo].[Packages] Where PackageScanStatus = 'Flagged') AS TotalPackagesScannedFlagged,
  (Select COUNT([Key]) From [dbo].[Packages] Where [IsLatestStable] = 1 And PackageScanStatus = 'Exempted') AS PackagesScannedExempted,
  (Select COUNT([Key]) From [dbo].[Packages] Where PackageScanStatus = 'Exempted') AS TotalPackagesScannedExempted,
  (Select COUNT([Key]) From [dbo].[Packages] Where [IsLatestStable] = 1 And PackageScanStatus = 'Investigate') AS PackagesScannedInvestigate,
  (Select COUNT([Key]) From [dbo].[Packages] Where PackageScanStatus = 'Investigate') AS TotalPackagesScannedInvestigate,
  (Select COUNT(ScanOverlaps) From (select count([ScanResultKey]) as ScanOverlaps from [dbo].[PackageScanResults] group by ScanResultKey having count(ScanResultKey) > 1) overlaps) AS TotalFileScanOverlaps,
  (Select COUNT([Key]) From [dbo].[ScanResults]) AS TotalFileScans

";

                                database.Connection.Open();
                                using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SingleRow))
                                {
                                    bool hasData = reader.Read();
                                    if (!hasData) return new AggregateStats();

                                    return new AggregateStats
                                    {
                                        UniquePackages = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                        TotalPackages = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                        Downloads = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                        PackagesReadyForReviewModeration = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                        TotalPackagesInModeration = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                        AverageModerationWaitTimeHours = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                        UpToDatePackages = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                        OlderThanOneYearPackages = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                        ApprovedPackages  = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                                        TotalApprovedPackages  = reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                                        ManuallyApprovedPackages  = reader.IsDBNull(10) ? 0 : reader.GetInt32(10),
                                        TotalManuallyApprovedPackages  = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                                        TrustedPackages  = reader.IsDBNull(12) ? 0 : reader.GetInt32(12),
                                        TotalTrustedPackages  = reader.IsDBNull(13) ? 0 : reader.GetInt32(13),
                                        TotalRejectedPackages  = reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
                                        ExemptedPackages  = reader.IsDBNull(15) ? 0 : reader.GetInt32(15),
                                        TotalExemptedPackages  = reader.IsDBNull(16) ? 0 : reader.GetInt32(16),
                                        UnknownPackages  = reader.IsDBNull(17) ? 0 : reader.GetInt32(17),
                                        TotalUnknownPackages  = reader.IsDBNull(18) ? 0 : reader.GetInt32(18),
                                        LatestPackagePrerelease  = reader.IsDBNull(19) ? 0 : reader.GetInt32(19),
                                        TotalUnlistedPackages  = reader.IsDBNull(20) ? 0 : reader.GetInt32(20),
                                        PackagesWithPackageSource  = reader.IsDBNull(21) ? 0 : reader.GetInt32(21),
                                        PackagesPassingVerification  = reader.IsDBNull(22) ? 0 : reader.GetInt32(22),
                                        PackagesFailingVerification  = reader.IsDBNull(23) ? 0 : reader.GetInt32(23),
                                        PackagesPassingValidation  = reader.IsDBNull(24) ? 0 : reader.GetInt32(24),
                                        PackagesFailingValidation  = reader.IsDBNull(25) ? 0 : reader.GetInt32(25),
                                        PackagesCached  = reader.IsDBNull(26) ? 0 : reader.GetInt32(26),
                                        TotalPackagesCached  = reader.IsDBNull(27) ? 0 : reader.GetInt32(27),
                                        PackagesCachedAvailable  = reader.IsDBNull(28) ? 0 : reader.GetInt32(28),
                                        TotalPackagesCachedAvailable  = reader.IsDBNull(29) ? 0 : reader.GetInt32(29),
                                        PackagesCachedInvestigate  = reader.IsDBNull(30) ? 0 : reader.GetInt32(30),
                                        TotalPackagesCachedInvestigate  = reader.IsDBNull(31) ? 0 : reader.GetInt32(31),
                                        PackagesScanned  = reader.IsDBNull(32) ? 0 : reader.GetInt32(32),
                                        TotalPackagesScanned  = reader.IsDBNull(33) ? 0 : reader.GetInt32(33),
                                        PackagesScannedNotFlagged  = reader.IsDBNull(34) ? 0 : reader.GetInt32(34),
                                        TotalPackagesScannedNotFlagged  = reader.IsDBNull(35) ? 0 : reader.GetInt32(35),
                                        PackagesScannedFlagged  = reader.IsDBNull(36) ? 0 : reader.GetInt32(36),
                                        TotalPackagesScannedFlagged  = reader.IsDBNull(37) ? 0 : reader.GetInt32(37),
                                        PackagesScannedExempted  = reader.IsDBNull(38) ? 0 : reader.GetInt32(38),
                                        TotalPackagesScannedExempted  = reader.IsDBNull(39) ? 0 : reader.GetInt32(39),
                                        PackagesScannedInvestigate  = reader.IsDBNull(40) ? 0 : reader.GetInt32(40),
                                        TotalPackagesScannedInvestigate  = reader.IsDBNull(41) ? 0 : reader.GetInt32(41),
                                        TotalFileScanOverlaps  = reader.IsDBNull(42) ? 0 : reader.GetInt32(42),
                                        TotalFileScans  = reader.IsDBNull(43) ? 0 : reader.GetInt32(43),
                                    };
                                }
                            }
                        }
                    });
        }
    }
}