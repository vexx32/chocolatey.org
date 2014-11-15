using NuGetGallery;
using WebActivator;

[assembly: PostApplicationStartMethod(typeof (SimpleInjectorInitializer), "Initialize")]

namespace NuGetGallery
{
    using System.Reflection;
    using SimpleInjector;
    using System.Web.Mvc;
    using SimpleInjector.Integration.Web.Mvc;

    public static class SimpleInjectorInitializer
    {
        /// <summary>Initialize the container and register it as MVC3 Dependency Resolver.</summary>
        public static void Initialize()
        {
            // Did you know the container can diagnose your configuration? Go to: http://bit.ly/YE8OJj.
            var container = new SimpleInjector.Container();
            container.Options.AllowOverridingRegistrations = true;

            var originalConstructorResolutionBehavior = container.Options.ConstructorResolutionBehavior;
            container.Options.ConstructorResolutionBehavior = new SimpleInjectorContainerResolutionBehavior(originalConstructorResolutionBehavior);

            InitializeContainer(container);

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            container.RegisterMvcAttributeFilterProvider();

#if DEBUG
            //container.Verify();
#endif
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }

        private static void InitializeContainer(SimpleInjector.Container container)
        {
            var binding = new ContainerBinding();
            binding.RegisterComponents(container);
        }
    }
}