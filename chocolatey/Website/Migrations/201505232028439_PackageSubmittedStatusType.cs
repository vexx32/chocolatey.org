namespace NuGetGallery.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class PackageSubmittedStatusType : DbMigration
    {
        public override void Up()
        {
            AddColumn("Packages", "SubmittedStatus", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("Packages", "SubmittedStatus");
        }
    }
}
