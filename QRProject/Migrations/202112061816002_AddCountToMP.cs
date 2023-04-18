namespace QRProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCountToMP : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MoreProducts", "count", c => c.Int(nullable: false));
            AddColumn("dbo.Products", "count", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "count");
            DropColumn("dbo.MoreProducts", "count");
        }
    }
}
