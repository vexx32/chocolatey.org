namespace NuGetGallery
{
    using System.Collections.Generic;

    public class PackageModerationComparer : IComparer<Package>
    {
        // -1 left < right
        //  0 left == right
        //  1 right < left

        public int Compare(Package left, Package right)
        {
            if (left == null || right == null)
            {
                if (ReferenceEquals(left, right))
                {
                    return 0;
                }
                return left == null ? -1 : 1;
            }

            if (left.Status == PackageStatusType.Submitted && right.Status == PackageStatusType.Submitted)
            {
                var leftStatus = GetPackageModerationStatus(left);
                var rightStatus = GetPackageModerationStatus(right);
                if (leftStatus != rightStatus)
                {
                    return leftStatus < rightStatus ? -1 : 1;
                }

                switch (leftStatus)
                {
                    case 0:  
                        //updated aka resubmitted
                    case 1:
                        //submitted
                        return left.LastUpdated <= right.LastUpdated ? -1 : 1;
                    case 2:
                        return left.ReviewedDate.GetValueOrDefault() >= right.ReviewedDate.GetValueOrDefault() ? -1 : 1;
                }
            }
            else
            {
                if (left.Status == PackageStatusType.Submitted)
                {
                    return -1;
                }
                else if (right.Status == PackageStatusType.Submitted)
                {
                    return 1;
                }
                
                return left.DownloadCount >= right.DownloadCount ? -1 : 1;
            }

            
            return Comparer<Package>.Default.Compare(left, right);
        }

        private static int GetPackageModerationStatus(Package package)
        {
            if (!package.ReviewedDate.HasValue) return 1;
            if (package.LastUpdated > package.ReviewedDate) return 0;

            return 2;
        }
    }
}