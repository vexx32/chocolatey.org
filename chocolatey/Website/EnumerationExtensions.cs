namespace NuGetGallery
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc;

    public static class EnumerationExtensions
    {
        public static IEnumerable<SelectListItem> GetEnumerationItems(this Enum enumeration)
        {
            var listItems = Enum
                .GetValues(enumeration.GetType())
                .OfType<Enum>()
                .Select(e =>
                        new SelectListItem
                            {
                                Text = e.GetDescriptionOrValue(),
                                Value = e.ToString(),
                                Selected = e.Equals(enumeration)
                            });

            return listItems;
        }

        /// <summary>
        ///   Gets the description [Description("")] or ToString() value of an enumeration.
        /// </summary>
        /// <param name="enumeration">The enumeration item.</param>
        public static string GetDescriptionOrValue(this Enum enumeration)
        {
            string description = enumeration.ToString();

            Type type = enumeration.GetType();
            MemberInfo[] memInfo = type.GetMember(description);

            if (memInfo != null && memInfo.Length > 0)
            {
                var attrib = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>().SingleOrDefault();

                if (attrib != null)
                {
                    description = attrib.Description;
                }
            }

            return description;
        }

    }
}