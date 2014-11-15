using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Elmah;
using Elmah.Contrib.Mvc;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using NuGetGallery.Jobs;
using NuGetGallery.Migrations;
using StackExchange.Profiling;
using StackExchange.Profiling.MVCHelpers;
using WebBackgrounder;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NuGetGallery.AppActivator), "PreStart")]
[assembly: WebActivator.PostApplicationStartMethod(typeof(NuGetGallery.AppActivator), "PostStart")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(NuGetGallery.AppActivator), "Stop")]

namespace NuGetGallery
{
    using System.Configuration;
    using System.Data.Entity;
    using MvcOverrides;

    public static class AppActivator
    {
        private static JobManager _jobManager;

        public static void PreStart()
        {
            MiniProfilerPreStart();
        }

        public static void PostStart()
        {
            MiniProfilerPostStart();
            //todo: this is how database is automatically updated
            //DbMigratorPostStart();
            BackgroundJobsPostStart();
            AppPostStart();
            DynamicDataPostStart();
        }

        public static void Stop()
        {
            BackgroundJobsStop();
        }

        private static void AppPostStart()
        {
            RegisterGlobalFilters(GlobalFilters.Filters);

            Routes.RegisterRoutes(RouteTable.Routes);

#if !DEBUG
                Database.SetInitializer<EntitiesContext>(null);
#endif

            ValueProviderFactories.Factories.Add(new HttpHeaderValueProviderFactory());
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ElmahHandleErrorAttribute());

            if (ConfigurationManager.AppSettings.Get("ForceSSL").Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase))
            {
                filters.Add(new RequireHttpsAppHarborAttribute());
            }
        }

        //private static void SetCustomRouteHandler()
        //{
        //     var routes = RouteTable.Routes.OfType<Route>().Where(x => x.RouteHandler is MvcRouteHandler);
        //    foreach (var route in routes)
        //    {
        //        route.RouteHandler = new CustomMvcRouteHandler();
        //    }
        //}

        private static void BackgroundJobsPostStart()
        {
            var jobs = new IJob[] { 
                new UpdateStatisticsJob(TimeSpan.FromMinutes(5), () => new EntitiesContext(), timeout: TimeSpan.FromMinutes(5)),
                new WorkItemCleanupJob(TimeSpan.FromDays(1), () => new EntitiesContext(), timeout: TimeSpan.FromDays(4)),
                new LuceneIndexingJob(TimeSpan.FromMinutes(10), timeout: TimeSpan.FromMinutes(2)),
            };
            var jobCoordinator = new WebFarmJobCoordinator(new EntityWorkItemRepository(() => new EntitiesContext()));
            _jobManager = new JobManager(jobs, jobCoordinator)
            {
                RestartSchedulerOnFailure = true
            };
            _jobManager.Fail(e => ErrorLog.GetDefault(null).Log(new Error(e)));
            _jobManager.Start();
        }

        private static void BackgroundJobsStop()
        {
            _jobManager.Dispose();
        }

        private static void DbMigratorPostStart()
        {
            var dbMigrator = new DbMigrator(new MigrationsConfiguration());
            // After upgrading to EF 4.3 and MiniProfile 1.9, there is a bug that causes several 
            // 'Invalid object name 'dbo.__MigrationHistory' to be thrown when the database is first created; 
            // it seems these can safely be ignored, and the database will still be created.
            dbMigrator.Update();
        }

        private static void DynamicDataPostStart()
        {
            DynamicDataEFCodeFirst.Registration.Register(RouteTable.Routes);
        }

        private static void MiniProfilerPreStart()
        {
            MiniProfilerEF.Initialize();
            DynamicModuleUtility.RegisterModule(typeof(MiniProfilerStartupModule));
            GlobalFilters.Filters.Add(new ProfilingActionFilter());
        }

        private static void MiniProfilerPostStart()
        {
            var copy = ViewEngines.Engines.ToList();
            ViewEngines.Engines.Clear();
            foreach (var item in copy) ViewEngines.Engines.Add(new ProfilingViewEngine(item));
        }

        private class MiniProfilerStartupModule : IHttpModule
        {
            public void Init(HttpApplication context)
            {
                context.BeginRequest += (sender, e) => MiniProfiler.Start();

                context.AuthorizeRequest += (sender, e) =>
                {
                    bool stopProfiling;
                    var httpContext = HttpContext.Current;

                    if (httpContext == null)
                        stopProfiling = true;
                    else
                    {
                        // Temporarily removing until we figure out the hammering of request we saw.
                        //var userCanProfile = httpContext.User != null && HttpContext.Current.User.IsInRole(Const.AdminRoleName);
                        var requestIsLocal = httpContext.Request.IsLocal;

                        //stopProfiling = !userCanProfile && !requestIsLocal
                        stopProfiling = !requestIsLocal;
                    }

                    if (stopProfiling)
                        MiniProfiler.Stop(true);
                };

                context.EndRequest += (sender, e) => MiniProfiler.Stop();
            }

            public void Dispose()
            {
            }
        }
    }
}