namespace NuGetGallery.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class UpdatePackageFiles : DbMigration
    {
        public override void Up()
        {
            AlterColumn("PackageFiles", "FileContent", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("PackageFiles", "FileContent", c => c.String(nullable: false));
        }
    }
}
