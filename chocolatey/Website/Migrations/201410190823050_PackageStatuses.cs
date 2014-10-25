namespace NuGetGallery.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class PackageStatuses : DbMigration
    {
        public override void Up()
        {
            AddColumn("Packages", "Status", c => c.String(maxLength: 100));
            AddColumn("Packages", "ReviewedDate", c => c.DateTime());
            AddColumn("Packages", "ApprovedDate", c => c.DateTime());
            AddColumn("Packages", "ReviewedById", c => c.Int());
            AddForeignKey("Packages", "ReviewedById", "Users", "Key");
            CreateIndex("Packages", "ReviewedById");
        }
        
        public override void Down()
        {
            DropIndex("Packages", new[] { "ReviewedById" });
            DropForeignKey("Packages", "ReviewedById", "Users");
            DropColumn("Packages", "ReviewedById");
            DropColumn("Packages", "ApprovedDate");
            DropColumn("Packages", "ReviewedDate");
            DropColumn("Packages", "Status");
        }
    }
}
