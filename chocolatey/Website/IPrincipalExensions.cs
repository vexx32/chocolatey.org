namespace NuGetGallery
{
    using System.Security.Principal;

    public static class IPrincipalExensions
    {
        public static bool IsAdmin(this IPrincipal user)
        {
            if (user == null || user.Identity == null) return false;
           
            return user.IsInRole(Constants.AdminRoleName);
        }

        public static bool IsModerator(this IPrincipal user)
        {
            if (user == null || user.Identity == null) return false;
           
            return user.IsInRole(Constants.ModeratorsRoleName) || IsAdmin(user);
        }

        public static bool IsReviewer(this IPrincipal user)
        {
            if (user == null || user.Identity == null) return false;
           
            return user.IsInRole(Constants.ReviewersRoleName);
        }

        public static bool IsInAnyModerationRole(this IPrincipal user)
        {
            return IsReviewer(user) || IsModerator(user) || IsAdmin(user);
        }
    }
}