namespace Zing.Framework.Localize
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Web.Mvc;
    using Zing.Framework.Utility;
    using Zing.Framework.Utility.Extensions;

    internal class LocalizationService : ILocalizationService
    {
        private static readonly IDictionary<string, ResourceBase> cache = new Dictionary<string, ResourceBase>(StringComparer.OrdinalIgnoreCase);
        private static readonly ReaderWriterLockSlim syncLock = new ReaderWriterLockSlim();

        private readonly ResourceBase resource;

        public LocalizationService(string resourceName, CultureInfo culture)
        {
            Guard.IsNotNullOrEmpty(resourceName, "resourceName");
            Guard.IsNotNull(culture, "culture");

            resource = DetectResource("~/App_GlobalResources", resourceName, culture);
        }

        public string One(string key)
        {
            return resource.GetByKey(key);
        }

        public bool IsDefault
        {
            get
            {
                return resource.IsDefault;
            }
        }

        public IDictionary<string, string> All()
        {
            return resource.GetAll();
        }

        private static ResourceBase DetectResource(string resourceLocation, string resourceName, CultureInfo culture)
        {
            string cacheKey = resourceName + ":" + culture;

            ResourceBase resource;

            using (syncLock.ReadAndWrite())
            {
                if (!cache.TryGetValue(cacheKey, out resource))
                {
                    using (syncLock.Write())
                    {
                        if (!cache.TryGetValue(cacheKey, out resource))
                        {
                            resource = CreateResource(resourceName, culture, resourceLocation);

                            cache.Add(cacheKey, resource);
                        }
                    }
                }
            }

            return resource;
        }

        private static ResourceBase CreateResource(string resourceName, CultureInfo culture, string resourceLocation)
        {
            Func<string, string> fixResourceName = c => resourceName + ((c != null) ? "." + c : string.Empty) + ".resx";

            IVirtualPathProvider vpp = DependencyResolver.Current.GetService<IVirtualPathProvider>();

            // First try the file path Resource.fr-CA.resx
            string fullResourcePath = vpp.CombinePaths(resourceLocation, fixResourceName(culture.ToString()));
            bool exists = vpp.FileExists(fullResourcePath);

            // If not found, try Resource.fr.resx
            if (!exists)
            {
                fullResourcePath = vpp.CombinePaths(resourceLocation, fixResourceName(culture.TwoLetterISOLanguageName));
                exists = vpp.FileExists(fullResourcePath);
            }

            // If nothing is found try Resource.resx
            if (!exists)
            {
                fullResourcePath = vpp.CombinePaths(resourceLocation, fixResourceName(null));
                exists = vpp.FileExists(fullResourcePath);
            }

            ResourceBase resource = exists ?
                                    new ResXResource(DependencyResolver.Current.GetService<IPathResolver>().Resolve(fullResourcePath)) :
                                    new EmbeddedResource(resourceName, culture) as ResourceBase;

            return resource;
        }
    }
}