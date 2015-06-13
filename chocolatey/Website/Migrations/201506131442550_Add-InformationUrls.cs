namespace NuGetGallery.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddInformationUrls : DbMigration
    {
        public override void Up()
        {
            AddColumn("Packages", "ProjectSourceUrl", c => c.String(maxLength: 400));
            AddColumn("Packages", "PackageSourceUrl", c => c.String(maxLength: 400));
            AddColumn("Packages", "DocsUrl", c => c.String(maxLength: 400));
            AddColumn("Packages", "MailingListUrl", c => c.String(maxLength: 400));
            AddColumn("Packages", "BugTrackerUrl", c => c.String(maxLength: 400));
        }
        
        public override void Down()
        {
            DropColumn("Packages", "BugTrackerUrl");
            DropColumn("Packages", "MailingListUrl");
            DropColumn("Packages", "DocsUrl");
            DropColumn("Packages", "PackageSourceUrl");
            DropColumn("Packages", "ProjectSourceUrl");
        }
    }
}
