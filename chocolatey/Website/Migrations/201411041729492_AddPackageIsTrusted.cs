namespace NuGetGallery.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddPackageIsTrusted : DbMigration
    {
        public override void Up()
        {
            AddColumn("PackageRegistrations", "IsTrusted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("PackageRegistrations", "IsTrusted");
        }
    }
}
