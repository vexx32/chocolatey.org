namespace NuGetGallery
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using AnglicanGeek.MarkdownMailer;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using SimpleInjector;

    /// <summary>
    ///   The main inversion container registration for the application. Look for other container bindings in client projects.
    /// </summary>
    public class ContainerBinding
    {
        /// <summary>
        ///   Loads the module into the kernel.
        /// </summary>
        public void RegisterComponents(SimpleInjector.Container container)
        {
            IConfiguration configuration = new Configuration();
            container.Register<IConfiguration>(() => configuration, Lifestyle.Singleton);

            Lazy<GallerySetting> gallerySetting = new Lazy<GallerySetting>(() =>
            {
                using (var entitiesContext = new EntitiesContext())
                {
                    var settingsRepo = new EntityRepository<GallerySetting>(entitiesContext);
                    return settingsRepo.GetAll().FirstOrDefault();
                }
            });

            container.Register<GallerySetting>(()=> gallerySetting.Value);
            //Bind<GallerySetting>().ToMethod(c => gallerySetting.Value);

            container.RegisterPerWebRequest<ISearchService, LuceneSearchService>();

            container.RegisterPerWebRequest<IEntitiesContext>(() => new EntitiesContext());
            container.RegisterPerWebRequest<IEntityRepository<User>, EntityRepository<User>>();
            container.RegisterPerWebRequest<IEntityRepository<PackageRegistration>, EntityRepository<PackageRegistration>>();
            container.RegisterPerWebRequest<IEntityRepository<Package>, EntityRepository<Package>>();
            container.RegisterPerWebRequest<IEntityRepository<PackageAuthor>, EntityRepository<PackageAuthor>>();
            container.RegisterPerWebRequest<IEntityRepository<PackageFramework>, EntityRepository<PackageFramework>>();
            container.RegisterPerWebRequest<IEntityRepository<PackageDependency>, EntityRepository<PackageDependency>>();
            container.RegisterPerWebRequest<IEntityRepository<PackageFile>, EntityRepository<PackageFile>>();
            container.RegisterPerWebRequest<IEntityRepository<PackageStatistics>, EntityRepository<PackageStatistics>>();
            container.RegisterPerWebRequest<IEntityRepository<PackageOwnerRequest>, EntityRepository<PackageOwnerRequest>>();

            container.RegisterPerWebRequest<IUserService, UserService>();
            container.RegisterPerWebRequest<IPackageService, PackageService>();
            container.RegisterPerWebRequest<ICryptographyService, CryptographyService>();

            container.Register<IFormsAuthenticationService, FormsAuthenticationService>(Lifestyle.Singleton);

            container.RegisterPerWebRequest<IControllerFactory, NuGetControllerFactory>();
            container.RegisterPerWebRequest<IIndexingService, LuceneIndexingService>();
            container.RegisterPerWebRequest<INuGetExeDownloaderService, NuGetExeDownloaderService>();

            Lazy<IMailSender> mailSenderThunk = new Lazy<IMailSender>(() =>
            {
                var settings = container.GetInstance<GallerySetting>();
                if (settings.UseSmtp)
                {
                    var mailSenderConfiguration = new MailSenderConfiguration()
                    {
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        Host = settings.SmtpHost,
                        Port = settings.SmtpPort,
                        EnableSsl = configuration.SmtpEnableSsl,
                    };

                    if (!String.IsNullOrWhiteSpace(settings.SmtpUsername))
                    {
                        mailSenderConfiguration.UseDefaultCredentials = false;
                        mailSenderConfiguration.Credentials = new NetworkCredential(
                            settings.SmtpUsername,
                            settings.SmtpPassword);
                    }

                    return new NuGetGallery.MailSender(mailSenderConfiguration);
                }
                else
                {
                    var mailSenderConfiguration = new MailSenderConfiguration()
                    {
                        DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                        PickupDirectoryLocation = HostingEnvironment.MapPath("~/App_Data/Mail")
                    };

                    return new NuGetGallery.MailSender(mailSenderConfiguration);
                }
            });

            container.Register<IMailSender>(()=> mailSenderThunk.Value,Lifestyle.Singleton);

            container.Register<IMessageService, MessageService>(Lifestyle.Singleton);

            container.RegisterPerWebRequest<IPrincipal>(() => HttpContext.Current.User);
            //Bind<IPrincipal>().ToMethod(context => HttpContext.Current.User);

            switch (configuration.PackageStoreType)
            {
                case PackageStoreType.FileSystem:
                case PackageStoreType.NotSpecified:
                    container.Register<IFileSystemService, FileSystemService>(Lifestyle.Singleton);
                    container.Register<IFileStorageService, FileSystemFileStorageService>(Lifestyle.Singleton);
                    break;
                case PackageStoreType.AzureStorageBlob:
                    container.Register<ICloudBlobClient>(() => new CloudBlobClientWrapper(new CloudBlobClient(
                            new Uri(configuration.AzureStorageBlobUrl, UriKind.Absolute),
                            new StorageCredentialsAccountAndKey(configuration.AzureStorageAccountName, configuration.AzureStorageAccessKey)
                            ))
                        ,Lifestyle.Singleton);
                    container.Register<IFileStorageService, CloudBlobFileStorageService>(Lifestyle.Singleton);
                    break;
                case PackageStoreType.AmazonS3Storage:
                    container.Register<IAmazonS3Client, AmazonS3ClientWrapper>(Lifestyle.Singleton);
                    container.Register<IFileStorageService, AmazonS3FileStorageService>(Lifestyle.Singleton);
                    break;
            }

            container.Register<IPackageFileService, PackageFileService>(Lifestyle.Singleton);
            container.Register<IUploadFileService, UploadFileService>();

            // todo: bind all package curators by convention
            container.Register<IAutomaticPackageCurator, WebMatrixPackageCurator>(Lifestyle.Singleton);
            container.Register<IAutomaticPackageCurator, Windows8PackageCurator>(Lifestyle.Singleton);

            // todo: bind all commands by convention
            container.RegisterPerWebRequest<IAutomaticallyCuratePackageCommand, AutomaticallyCuratePackageCommand>();
            container.RegisterPerWebRequest<ICreateCuratedPackageCommand, CreateCuratedPackageCommand>();
            container.RegisterPerWebRequest<IDeleteCuratedPackageCommand, DeleteCuratedPackageCommand>();
            container.RegisterPerWebRequest<IModifyCuratedPackageCommand, ModifyCuratedPackageCommand>();

            // todo: bind all queries by convention
            container.RegisterPerWebRequest<ICuratedFeedByKeyQuery, CuratedFeedByKeyQuery>();
            container.RegisterPerWebRequest<ICuratedFeedByNameQuery, CuratedFeedByNameQuery>();
            container.RegisterPerWebRequest<ICuratedFeedsByManagerQuery, CuratedFeedsByManagerQuery>();
            container.RegisterPerWebRequest<IPackageRegistrationByKeyQuery, PackageRegistrationByKeyQuery>();
            container.RegisterPerWebRequest<IPackageRegistrationByIdQuery, PackageRegistrationByIdQuery>();
            container.RegisterPerWebRequest<IUserByUsernameQuery, UserByUsernameQuery>();
            container.RegisterPerWebRequest<IPackageIdsQuery, PackageIdsQuery>();
            container.RegisterPerWebRequest<IPackageVersionsQuery, PackageVersionsQuery>();

            container.RegisterPerWebRequest<IAggregateStatsService, AggregateStatsService>();

            RegisterChocolateySpecific(container);
        }


        private void RegisterChocolateySpecific(SimpleInjector.Container container)
        {
            container.RegisterPerWebRequest<IUserSiteProfilesService, UserSiteProfilesService>();
            container.RegisterPerWebRequest<IEntityRepository<UserSiteProfile>, EntityRepository<UserSiteProfile>>();
        }
    }
}