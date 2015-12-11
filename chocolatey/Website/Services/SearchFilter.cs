
using System.Collections.Generic;

namespace NuGetGallery
{
    public class SearchFilter
    {
        public string SearchTerm { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }

        public bool IncludePrerelease { get; set; }

        public SortProperty SortProperty { get; set; }

        public SortDirection SortDirection { get; set; }

        /// <summary>
        /// Determines if only this is a count only query and does not process the source queryable.
        /// </summary>
        public bool CountOnly { get; set; }

        public bool IncludeAllVersions { get; set; }

        public IDictionary<string, string> QueryTerms { get; set; }
        public bool IsValid { get; set; }
        public SearchFilterInvalidReason FilterInvalidReason { get; set; }

        public static SearchFilter Empty()
        {
            return new SearchFilter
            {
                IsValid = false,
                FilterInvalidReason = SearchFilterInvalidReason.Unknown,
            };
        }
    }

    public enum SearchFilterInvalidReason
    {
        Unknown,
        DueToAllVersionsRequested
    }

    public enum SortProperty
    {
        Relevance,
        DownloadCount,
        DisplayName,
        Recent,
    }

    public enum SortDirection
    {
        Ascending,
        Descending,
    }
}