namespace QRProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class first : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApplyingFors",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        PhoneNumber = c.String(),
                        Addres = c.String(),
                        Name = c.String(),
                        CreateDtae = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.baladcheeProducts",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        Price = c.String(),
                        CreateDate = c.DateTime(),
                        SmsCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CategoryWords",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CreateDate = c.DateTime(nullable: false),
                        Word = c.String(),
                        ClienId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.ClienId)
                .Index(t => t.ClienId);
            
            CreateTable(
                "dbo.Clients",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Title = c.String(),
                        Content = c.String(),
                        BigContent = c.String(),
                        PhonNumber = c.String(),
                        Address = c.String(),
                        Email = c.String(),
                        Instagram = c.String(),
                        SaleTitle = c.String(),
                        Lng = c.String(),
                        Lat = c.String(),
                        Telegram = c.String(),
                        QrLocation = c.String(),
                        PhotoAddress = c.String(),
                        mobile = c.String(),
                        ownerName = c.String(),
                        ContractPic = c.String(),
                        isOnline = c.Int(nullable: false),
                        OurTicket = c.String(),
                        SaleCode = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        QrImageAddres = c.String(),
                        Fisrtcolor = c.String(),
                        Seccondcolor = c.String(),
                        Percent = c.String(),
                        VideoAddress = c.String(),
                        AppAddress = c.String(),
                        smsCount = c.Int(),
                        ZarinCode = c.String(),
                        UserId = c.String(maxLength: 128),
                        City = c.String(),
                        BorunshahriPrice = c.String(),
                        DarunshahriPrice = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CreateDate = c.DateTime(),
                        SubjobId = c.String(maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subjobs", t => t.SubjobId)
                .Index(t => t.SubjobId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Subjobs",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        SubjobName = c.String(),
                        CreatDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Contacts",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ClienId = c.String(maxLength: 128),
                        CreateDate = c.DateTime(nullable: false),
                        PhoneNumber = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.ClienId)
                .Index(t => t.ClienId);
            
            CreateTable(
                "dbo.CustomerClubs",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ClienId = c.String(maxLength: 128),
                        CreateDate = c.DateTime(nullable: false),
                        PhoneNumber = c.String(),
                        Ip = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.ClienId)
                .Index(t => t.ClienId);
            
            CreateTable(
                "dbo.MoreProducts",
                c => new
                    {
                        MoreProductId = c.String(nullable: false, maxLength: 128),
                        pTitle = c.String(),
                        pContent = c.String(),
                        pPicAddres = c.String(),
                        Takhfif = c.String(),
                        NamayesheGheymat = c.String(),
                        DasteBandi = c.String(),
                        SefareshMahsool = c.String(),
                        pCreateDate = c.DateTime(),
                        MoreProductSocialMediaLink = c.String(),
                        Display = c.String(),
                        GheymatFeli = c.String(),
                        GheymatGhabli = c.String(),
                        ClienId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.MoreProductId)
                .ForeignKey("dbo.Clients", t => t.ClienId)
                .Index(t => t.ClienId);
            
            CreateTable(
                "dbo.Opinions",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ClienId = c.String(maxLength: 128),
                        CreateDate = c.DateTime(nullable: false),
                        Content = c.String(),
                        ConnectContent = c.String(),
                        Ip = c.String(),
                        Count = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.ClienId)
                .Index(t => t.ClienId);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        pId = c.String(nullable: false, maxLength: 128),
                        pTitle = c.String(),
                        pContent = c.String(),
                        pPicAddres = c.String(),
                        Takhfif = c.String(),
                        NamayesheGheymat = c.String(),
                        SefareshMahsool = c.String(),
                        pCreateDate = c.DateTime(),
                        ProductSocialMediaLink = c.String(),
                        Display = c.String(),
                        GheymatFeli = c.String(),
                        GheymatGhabli = c.String(),
                        ClienId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.pId)
                .ForeignKey("dbo.Clients", t => t.ClienId)
                .Index(t => t.ClienId);
            
            CreateTable(
                "dbo.SMSBoxes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ClienId = c.String(maxLength: 128),
                        CreateDate = c.DateTime(nullable: false),
                        Content = c.String(),
                        isShow = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.ClienId)
                .Index(t => t.ClienId);
            
            CreateTable(
                "dbo.statistics",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ClienId = c.String(maxLength: 128),
                        CreateDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.ClienId)
                .Index(t => t.ClienId);
            
            CreateTable(
                "dbo.factors",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CreateDate = c.DateTime(nullable: false),
                        Price = c.String(),
                        smsSaleCount = c.String(),
                        Description = c.String(),
                        IsFinaly = c.Boolean(nullable: false),
                        ClienId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.ClienId)
                .Index(t => t.ClienId);
            
            CreateTable(
                "dbo.moshtaris",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CreateDate = c.DateTime(nullable: false),
                        Name = c.String(),
                        PhoneNumber = c.String(),
                        Address = c.String(),
                        PostalCode = c.String(),
                        LocationLatitude = c.String(),
                        LocationLongitude = c.String(),
                        City = c.String(),
                        EshterakCode = c.String(),
                        EshterakCodeCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PardakhtVitrins",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CreateDate = c.DateTime(nullable: false),
                        start = c.DateTime(nullable: false),
                        End = c.DateTime(),
                        testTime = c.DateTime(),
                        pardakht = c.Int(nullable: false),
                        FirstSale = c.Int(nullable: false),
                        ClienId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.ClienId)
                .Index(t => t.ClienId);
            
            CreateTable(
                "dbo.RizSefareshats",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ProductId = c.String(maxLength: 128),
                        MoreProducstId = c.String(maxLength: 128),
                        SefareshatId = c.String(maxLength: 128),
                        CreateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MoreProducts", t => t.MoreProducstId)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .ForeignKey("dbo.Sefareshats", t => t.SefareshatId)
                .Index(t => t.ProductId)
                .Index(t => t.MoreProducstId)
                .Index(t => t.SefareshatId);
            
            CreateTable(
                "dbo.Sefareshats",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Tozihat = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        TakhfifCode = c.String(),
                        Tracking_Code = c.String(),
                        FactorAddres = c.String(),
                        isSeen = c.Boolean(nullable: false),
                        RefId = c.String(),
                        isFinaly = c.Boolean(nullable: false),
                        Sum = c.Int(nullable: false),
                        DeliveryPrice = c.String(),
                        clientId = c.String(maxLength: 128),
                        moshtariId = c.String(maxLength: 128),
                        DarsadTakhfif = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clients", t => t.clientId)
                .ForeignKey("dbo.moshtaris", t => t.moshtariId)
                .Index(t => t.clientId)
                .Index(t => t.moshtariId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.SampleContents",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        JobName = c.String(),
                        BigContent = c.String(),
                        Content = c.String(),
                        Title = c.String(),
                        Address = c.String(),
                        PhoneNumber = c.String(),
                        Email = c.String(),
                        SaleTitle = c.String(),
                        lng = c.String(),
                        lat = c.String(),
                        FirstColor = c.String(),
                        SeccondColor = c.String(),
                        PhotoAddress = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.RizSefareshats", "SefareshatId", "dbo.Sefareshats");
            DropForeignKey("dbo.Sefareshats", "moshtariId", "dbo.moshtaris");
            DropForeignKey("dbo.Sefareshats", "clientId", "dbo.Clients");
            DropForeignKey("dbo.RizSefareshats", "ProductId", "dbo.Products");
            DropForeignKey("dbo.RizSefareshats", "MoreProducstId", "dbo.MoreProducts");
            DropForeignKey("dbo.PardakhtVitrins", "ClienId", "dbo.Clients");
            DropForeignKey("dbo.factors", "ClienId", "dbo.Clients");
            DropForeignKey("dbo.CategoryWords", "ClienId", "dbo.Clients");
            DropForeignKey("dbo.statistics", "ClienId", "dbo.Clients");
            DropForeignKey("dbo.SMSBoxes", "ClienId", "dbo.Clients");
            DropForeignKey("dbo.Products", "ClienId", "dbo.Clients");
            DropForeignKey("dbo.Opinions", "ClienId", "dbo.Clients");
            DropForeignKey("dbo.MoreProducts", "ClienId", "dbo.Clients");
            DropForeignKey("dbo.CustomerClubs", "ClienId", "dbo.Clients");
            DropForeignKey("dbo.Contacts", "ClienId", "dbo.Clients");
            DropForeignKey("dbo.Clients", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "SubjobId", "dbo.Subjobs");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Sefareshats", new[] { "moshtariId" });
            DropIndex("dbo.Sefareshats", new[] { "clientId" });
            DropIndex("dbo.RizSefareshats", new[] { "SefareshatId" });
            DropIndex("dbo.RizSefareshats", new[] { "MoreProducstId" });
            DropIndex("dbo.RizSefareshats", new[] { "ProductId" });
            DropIndex("dbo.PardakhtVitrins", new[] { "ClienId" });
            DropIndex("dbo.factors", new[] { "ClienId" });
            DropIndex("dbo.statistics", new[] { "ClienId" });
            DropIndex("dbo.SMSBoxes", new[] { "ClienId" });
            DropIndex("dbo.Products", new[] { "ClienId" });
            DropIndex("dbo.Opinions", new[] { "ClienId" });
            DropIndex("dbo.MoreProducts", new[] { "ClienId" });
            DropIndex("dbo.CustomerClubs", new[] { "ClienId" });
            DropIndex("dbo.Contacts", new[] { "ClienId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", new[] { "SubjobId" });
            DropIndex("dbo.Clients", new[] { "UserId" });
            DropIndex("dbo.CategoryWords", new[] { "ClienId" });
            DropTable("dbo.SampleContents");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Sefareshats");
            DropTable("dbo.RizSefareshats");
            DropTable("dbo.PardakhtVitrins");
            DropTable("dbo.moshtaris");
            DropTable("dbo.factors");
            DropTable("dbo.statistics");
            DropTable("dbo.SMSBoxes");
            DropTable("dbo.Products");
            DropTable("dbo.Opinions");
            DropTable("dbo.MoreProducts");
            DropTable("dbo.CustomerClubs");
            DropTable("dbo.Contacts");
            DropTable("dbo.Subjobs");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Clients");
            DropTable("dbo.CategoryWords");
            DropTable("dbo.baladcheeProducts");
            DropTable("dbo.ApplyingFors");
        }
    }
}
