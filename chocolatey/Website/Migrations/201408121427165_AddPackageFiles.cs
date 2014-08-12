namespace NuGetGallery.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddPackageFiles : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "PackageFiles",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        PackageKey = c.Int(nullable: false),
                        FilePath = c.String(nullable: false, maxLength: 500),
                        FileContent = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("Packages", t => t.PackageKey, cascadeDelete: true)
                .Index(t => t.PackageKey);
            
        }
        
        public override void Down()
        {
            DropIndex("PackageFiles", new[] { "PackageKey" });
            DropForeignKey("PackageFiles", "PackageKey", "Packages");
            DropTable("PackageFiles");
        }
    }
}
