namespace NuGetGallery.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class CourseProfile : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "CourseProfiles",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 255),
                        Url = c.String(maxLength: 255),
                        Image = c.String(maxLength: 400),
                        Username = c.String(),
                        Completed = c.Boolean(nullable: false),
                        ModOne = c.Boolean(nullable: false),
                        ModTwo = c.Boolean(nullable: false),
                        ModThree = c.Boolean(nullable: false),
                        ModFour = c.Boolean(nullable: false),
                        ModFive = c.Boolean(nullable: false),
                        ModSix = c.Boolean(nullable: false),
                        ModSeven = c.Boolean(nullable: false),
                        ModEight = c.Boolean(nullable: false),
                        ModNine = c.Boolean(nullable: false),
                        ModTen = c.Boolean(nullable: false),
                        ModEleven = c.Boolean(nullable: false),
                        ModTwelve = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Key);
        }
        
        public override void Down()
        {
            DropTable("CourseProfiles");
        }
    }
}
