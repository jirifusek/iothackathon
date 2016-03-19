namespace Hackathon.BeesPortal.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DataSegments", "DeviceId", c => c.String(nullable: false, maxLength: 10, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DataSegments", "DeviceId", c => c.String(nullable: false, maxLength: 15, unicode: false));
        }
    }
}
