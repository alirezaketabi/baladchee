namespace QRProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFurushHuzuri : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Clients", "FurushHuzuri", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Clients", "FurushHuzuri");
        }
    }
}
