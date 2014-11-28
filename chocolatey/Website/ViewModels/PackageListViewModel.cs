using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using StackExchange.Profiling;

namespace NuGetGallery
{
    public class PackageListViewModel
    {
        public PackageListViewModel(IQueryable<Package> packages,
            string searchTerm,
            string sortOrder,
            int totalCount,
            int pageIndex,
            int pageSize,
            UrlHelper url,
            bool includePrerelease, 
            bool moderatorQueue,
            int updatedCount,
            int submittedCount,
            int waitingCount)
        {
            // TODO: Implement actual sorting
            IEnumerable<ListPackageItemViewModel> items;
            using (MiniProfiler.Current.Step("Querying and mapping packages to list"))
            {
                items = packages
                          .ToList()
                          .Select(pv => new ListPackageItemViewModel(pv, needAuthors: false));
            }
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
            SortOrder = sortOrder;
            SearchTerm = searchTerm;
            int pageCount = (TotalCount + PageSize - 1) / PageSize;

            var pager = new PreviousNextPagerViewModel<ListPackageItemViewModel>(
                items,
                PageIndex,
                pageCount,
                page => url.PackageList(page, sortOrder, searchTerm, includePrerelease, moderatorQueue)
            );
            Items = pager.Items;
            FirstResultIndex = 1 + (PageIndex * PageSize);
            LastResultIndex = FirstResultIndex + Items.Count() - 1;
            Pager = pager;
            IncludePrerelease = includePrerelease ? "true" : null;
            ModeratorQueue = moderatorQueue ? "true" : null;
            ModerationUpdatedPackageCount = updatedCount;
            ModerationSubmittedPackageCount = submittedCount;
            ModerationWaitingPackageCount = waitingCount;
        }

        public int FirstResultIndex { get; set; }

        public IEnumerable<ListPackageItemViewModel> Items { get; private set; }

        public int LastResultIndex { get; set; }

        public IPreviousNextPager Pager { get; private set; }

        public int TotalCount { get; private set; }

        public string SearchTerm { get; private set; }

        public string SortOrder { get; private set; }

        public int PageIndex { get; private set; }

        public int PageSize { get; private set; }

        public string IncludePrerelease { get; private set; }

        public string ModeratorQueue { get; private set; }

        public int ModerationUpdatedPackageCount { get; private set; }
        public int ModerationSubmittedPackageCount { get; private set; }
        public int ModerationWaitingPackageCount { get; private set; }
    }
}