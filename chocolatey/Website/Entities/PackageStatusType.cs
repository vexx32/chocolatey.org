namespace NuGetGallery
{
    public enum PackageStatusType
    {
        Unknown,
        Submitted,
        Approved,
        Rejected,
        //certain package types, like prerelease get exempted
        Exempted,
    }
}