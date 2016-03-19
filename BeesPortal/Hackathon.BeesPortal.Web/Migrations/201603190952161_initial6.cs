namespace Hackathon.BeesPortal.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial6 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Notifications", "Text", c => c.String(nullable: false, maxLength: 128, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Notifications", "Text", c => c.String(nullable: false, maxLength: 10, unicode: false));
        }
    }
}
