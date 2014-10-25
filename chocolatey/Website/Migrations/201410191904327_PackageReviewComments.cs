namespace NuGetGallery.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class PackageReviewComments : DbMigration
    {
        public override void Up()
        {
            AddColumn("Packages", "ReviewComments", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("Packages", "ReviewComments");
        }
    }
}
