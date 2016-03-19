namespace Hackathon.BeesPortal.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial5 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Apiaries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 256),
                        Label = c.String(maxLength: 256, unicode: false),
                        Location = c.String(maxLength: 128, unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Hives",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ApiaryId = c.Int(nullable: false),
                        SigfoxId = c.String(maxLength: 5, unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SigfoxId = c.String(maxLength: 5, unicode: false),
                        Severity = c.String(nullable: false, maxLength: 10, unicode: false),
                        Text = c.String(nullable: false, maxLength: 10, unicode: false),
                        DateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.DataSegments", "SigfoxId", c => c.String(maxLength: 5, unicode: false));
            DropColumn("dbo.DataSegments", "DeviceId");
            DropColumn("dbo.DataSegments", "CoordinateX");
            DropColumn("dbo.DataSegments", "CoordinateY");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DataSegments", "CoordinateY", c => c.Single());
            AddColumn("dbo.DataSegments", "CoordinateX", c => c.Single());
            AddColumn("dbo.DataSegments", "DeviceId", c => c.String(nullable: false, maxLength: 10, unicode: false));
            DropColumn("dbo.DataSegments", "SigfoxId");
            DropTable("dbo.Notifications");
            DropTable("dbo.Hives");
            DropTable("dbo.Apiaries");
        }
    }
}
