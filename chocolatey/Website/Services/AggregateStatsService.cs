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
	(Select COUNT([Key]) From [dbo].[Packages] Where [Status] = 'Submitted' AND SubmittedStatus <> 'Waiting') AS PackagesReadyForReview,
	(Select COUNT([Key]) From [dbo].[Packages] Where [Status] = 'Submitted') AS AllPackagesUnderModeration,
	(Select AVG(DATEDIFF(hour,Created,ApprovedDate)) From dbo.Packages Where [Status] = 'Approved' And ReviewedById is Not Null And Created >= DATEADD(DAY, -14, GETDATE())) AS AverageModerationWaitTime
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
                                    };
                                }
                            }
                        }
                    });
        }
    }
}