// Copyright 2011 - Present RealDimensions Software, LLC, the original 
// authors/contributors from ChocolateyGallery
// at https://github.com/chocolatey/chocolatey.org,
// and the authors/contributors of NuGetGallery 
// at https://github.com/NuGet/NuGetGallery
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Data.Services;
using System.Data.Services.Common;
using System.Data.Services.Providers;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using Microsoft.Data.OData.Query;
using Microsoft.Data.OData.Query.SyntacticAst;
using NuGetGallery.MvcOverrides;
using QueryInterceptor;

namespace NuGetGallery
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public abstract class FeedServiceBase<TPackage> : DataService<FeedContext<TPackage>>, IDataServiceStreamProvider, IServiceProvider, IDataServicePagingProvider
    {
        /// <summary>
        ///   Determines the maximum number of packages returned in a single page of an OData result.
        /// </summary>
        private const int MaxPageSize = 40;
        private readonly IEntitiesContext entities;
        private readonly IEntityRepository<Package> packageRepo;
        private readonly IConfiguration configuration;
        private HttpContextBase httpContext;
        private int _currentSkip;
        private object[] _continuationToken;
        private readonly Type _packageType;

        public FeedServiceBase()
            : this(
                DependencyResolver.Current.GetService<IEntitiesContext>(), DependencyResolver.Current.GetService<IEntityRepository<Package>>(), DependencyResolver.Current.GetService<IConfiguration>())
        {
        }

        protected FeedServiceBase(IEntitiesContext entities, IEntityRepository<Package> packageRepo, IConfiguration configuration)
        {
            this.entities = entities;
            this.packageRepo = packageRepo;
            this.configuration = configuration;
            _currentSkip = 0;
            _packageType = typeof(TPackage);
        }

        protected IEntitiesContext Entities { get { return entities; } }

        protected IEntityRepository<Package> PackageRepo { get { return packageRepo; } }

        protected IConfiguration Configuration { get { return configuration; } }

        protected internal virtual HttpContextBase HttpContext { get { return httpContext ?? new HttpContextWrapper(System.Web.HttpContext.Current); } set { httpContext = value; } }

        protected internal string SiteRoot
        {
            get
            {
                string siteRoot = Configuration.GetSiteRoot(UseHttps());
                return EnsureTrailingSlash(siteRoot);
            }
        }

        // This method is called only once to initialize service-wide policies.
        protected static void InitializeServiceBase(DataServiceConfiguration config)
        {
            config.SetServiceOperationAccessRule("Search", ServiceOperationRights.AllRead);
            config.SetServiceOperationAccessRule("FindPackagesById", ServiceOperationRights.AllRead);
            config.SetEntitySetAccessRule("Packages", EntitySetRights.AllRead);
            config.SetEntitySetPageSize("Packages", MaxPageSize);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
            config.UseVerboseErrors = true;
        }

        public void DeleteStream(object entity, DataServiceOperationContext operationContext)
        {
            throw new NotSupportedException();
        }

        public Stream GetReadStream(object entity, string etag, bool? checkETagForEquality, DataServiceOperationContext operationContext)
        {
            throw new NotSupportedException();
        }

        public abstract Uri GetReadStreamUri(object entity, DataServiceOperationContext operationContext);

        public string GetStreamContentType(object entity, DataServiceOperationContext operationContext)
        {
            return "application/zip";
        }

        public string GetStreamETag(object entity, DataServiceOperationContext operationContext)
        {
            return null;
        }

        public Stream GetWriteStream(object entity, string etag, bool? checkETagForEquality, DataServiceOperationContext operationContext)
        {
            throw new NotSupportedException();
        }

        public string ResolveType(string entitySetName, DataServiceOperationContext operationContext)
        {
            throw new NotSupportedException();
        }

        public int StreamBufferSize { get { return 64000; } }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IDataServiceStreamProvider)) return this;
            //todo rr 20150614 look at turning this on for custom data service paging 
            //if (serviceType == typeof(IDataServicePagingProvider))
            //{
            //    return this;
            //}

            return null;
        }

        protected virtual IQueryable<Package> SearchCore(IQueryable<Package> packages, string searchTerm, string targetFramework, bool includePrerelease)
        {
            if (!includePrerelease) packages = packages.Where(p => !p.IsPrerelease);
            return packages.Search(searchTerm);
        }
       
        protected virtual bool UseHttps()
        {
            return AppHarbor.IsSecureConnection(HttpContext);
        }

        private static string EnsureTrailingSlash(string siteRoot)
        {
            if (!siteRoot.EndsWith("/", StringComparison.Ordinal)) siteRoot = siteRoot + '/';
            return siteRoot;
        }

        public void SetContinuationToken(IQueryable query, ResourceType resourceType, object[] continuationToken)
        {
            if (resourceType.FullName != _packageType.FullName) throw new ArgumentException("The paging provider can not construct a meaningful continuation token because its type is different from the ResourceType for which a continuation token is requested.");

            var materializedQuery = (query as IQueryable<TPackage>).ToList();
            var lastElement = materializedQuery.LastOrDefault();
            if (lastElement != null && materializedQuery.Count == MaxPageSize)
            {
                string packageId = _packageType.GetProperty("Id").GetValue(lastElement, null).ToString();
                string packageVersion = _packageType.GetProperty("Version").GetValue(lastElement, null).ToString();
                _continuationToken = new object[] { packageId, packageVersion, _currentSkip + Math.Min(materializedQuery.Count, MaxPageSize) };
            } else _continuationToken = null;
        }

        public object[] GetContinuationToken(IEnumerator enumerator)
        {
            return _continuationToken;
        }
    }
}
