namespace NuGetGallery.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddPackageTrustedFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("PackageRegistrations", "TrustedDate", c => c.DateTime());
            AddColumn("PackageRegistrations", "TrustedById", c => c.Int());
            AddForeignKey("PackageRegistrations", "TrustedById", "Users", "Key");
            CreateIndex("PackageRegistrations", "TrustedById");
        }
        
        public override void Down()
        {
            DropIndex("PackageRegistrations", new[] { "TrustedById" });
            DropForeignKey("PackageRegistrations", "TrustedById", "Users");
            DropColumn("PackageRegistrations", "TrustedById");
            DropColumn("PackageRegistrations", "TrustedDate");
        }
    }
}
