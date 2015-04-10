using System.Linq;

namespace NuGetGallery
{
    using System;
    using System.Data.Entity;
    using System.Data.Services;
    using System.ServiceModel.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    public class V2SubmittedFeed : FeedServiceBase<V2FeedPackage>
    {
        private const int FeedVersion = 2;
        private readonly string submittedStatus = PackageStatusType.Submitted.GetDescriptionOrValue();

        public V2SubmittedFeed()
        {
        }

        public V2SubmittedFeed(IEntitiesContext entities, IEntityRepository<Package> repo, IConfiguration configuration, ISearchService searchSvc)
            : base(entities, repo, configuration, searchSvc)
        {
        }

        protected override FeedContext<V2FeedPackage> CreateDataSource()
        {
            return new FeedContext<V2FeedPackage>
            {
                Packages = PackageRepo.GetAll()
                                .Where(p => p.StatusForDatabase == submittedStatus)
                                .WithoutVersionSort()
                                .ToV2FeedPackageQuery(GetSiteRoot())
            };
        }

        public static void InitializeService(DataServiceConfiguration config)
        {
            InitializeServiceBase(config);
        }

        [WebGet]
        public IQueryable<V2FeedPackage> Search(string searchTerm, string targetFramework, bool includePrerelease)
        {
            var packages = PackageRepo.GetAll()
                .Where(p => p.StatusForDatabase == PackageStatusType.Submitted.GetDescriptionOrValue());
            return SearchCore(packages, searchTerm, targetFramework, includePrerelease).ToV2FeedPackageQuery(GetSiteRoot());
        }

        [WebGet]
        public IQueryable<V2FeedPackage> FindPackagesById(string id)
        {
            return PackageRepo.GetAll().Include(p => p.PackageRegistration)
                                       .Where(p => p.PackageRegistration.Id.Equals(id, StringComparison.OrdinalIgnoreCase) && p.StatusForDatabase == submittedStatus)
                                       .ToV2FeedPackageQuery(GetSiteRoot());
        }

        public override Uri GetReadStreamUri(object entity, DataServiceOperationContext operationContext)
        {
            var package = (V2FeedPackage)entity;
            var urlHelper = new UrlHelper(new RequestContext(HttpContext, new RouteData()));

            string url = urlHelper.PackageDownload(FeedVersion, package.Id, package.Version);

            return new Uri(url, UriKind.Absolute);
        }

        private string GetSiteRoot()
        {
            return Configuration.GetSiteRoot(UseHttps());
        }
    }
}