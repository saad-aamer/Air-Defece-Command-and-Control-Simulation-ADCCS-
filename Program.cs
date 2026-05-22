using ADCCS_Web.Data;
using ADCCS_Web.Services;
using ADCCS_Web.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IRadarService, RadarService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ===== AUTO-CREATE DATABASE IF IT DOESN'T EXIST =====
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated(); // This creates the database if it doesn't exist
    try { context.Database.ExecuteSqlRaw("ALTER TABLE Targets ADD COLUMN AssetId INTEGER REFERENCES Assets(AssetId);"); } catch { }
    try { context.Database.ExecuteSqlRaw("ALTER TABLE DefensiveActions ADD COLUMN AssetId INTEGER REFERENCES Assets(AssetId);"); } catch { }
    try { context.Database.ExecuteSqlRaw("ALTER TABLE MissionLogs ADD COLUMN AssetId INTEGER REFERENCES Assets(AssetId);"); } catch { }

    try 
    { 
        context.Database.ExecuteSqlRaw(@"
            CREATE TRIGGER IF NOT EXISTS UpdateTargetStatusOnCompletion
            AFTER UPDATE OF StatusId ON DefensiveActions
            FOR EACH ROW
            WHEN NEW.StatusId = 2
            BEGIN
                UPDATE Targets
                SET IsActive = 0
                WHERE TargetId = NEW.TargetId;
            END;

            CREATE TRIGGER IF NOT EXISTS LogTargetDetection
            AFTER INSERT ON Targets
            FOR EACH ROW
            BEGIN
                INSERT INTO MissionLogs (MissionDate, EventTypeId, Description, SeverityId, TargetId, CreatedAt)
                VALUES (datetime('now', 'localtime'), 2, 'New target detected: ' || NEW.TargetCode, 1, NEW.TargetId, datetime('now', 'localtime'));
            END;

            CREATE TRIGGER IF NOT EXISTS LogActionIssued
            AFTER INSERT ON DefensiveActions
            FOR EACH ROW
            BEGIN
                INSERT INTO MissionLogs (MissionDate, EventTypeId, Description, SeverityId, UserId, TargetId, ActionId, CreatedAt, AssetId)
                VALUES (datetime('now', 'localtime'), 3, 'Defensive action issued for Target ID: ' || NEW.TargetId, 2, NEW.IssuedBy, NEW.TargetId, NEW.ActionId, datetime('now', 'localtime'), NEW.AssetId);
            END;
        "); 
    } catch { }

    if (!context.AssetTypes.Any())
    {
        context.Database.ExecuteSqlRaw("INSERT INTO \"AssetTypes\" (\"AssetTypeId\",\"Name\") VALUES (1,'Aircraft'), (2,'Missile'), (3,'Drone');");
    }
    if (!context.Assets.Any())
    {
        context.Database.ExecuteSqlRaw(@"INSERT INTO ""Assets"" (""AssetId"",""AssetName"",""AssetTypeId"",""MaxSpeed"",""MaxRange"") VALUES (1,'JF-17 Thunder Block III',1,1960.0,1352.0),
 (2,'F-16 Fighting Falcon',1,2410.0,1600.0),
 (3,'Mirage III ROSE',1,2350.0,1200.0),
 (4,'Mirage V Bahadur',1,2350.0,1300.0),
 (5,'F-7PG Skybolt',1,2175.0,1200.0),
 (6,'Saab 2000 AEW&C',1,680.0,3000.0),
 (7,'Erieye AEW&C',1,680.0,3200.0),
 (8,'K-8 Karakorum',1,800.0,800.0),
 (9,'Babur Cruise Missile',2,880.0,700.0),
 (10,'Ra''ad Air-Launched CM',2,950.0,350.0),
 (11,'Hatf-III Ghaznavi SRBM',2,5796.0,290.0),
 (12,'LY-80 (HQ-16) SAM',2,3000.0,40.0),
 (13,'Spada 2000 SAM',2,1400.0,25.0),
 (14,'AIM-120 AMRAAM',2,4248.0,160.0),
 (15,'SD-10A BVRAAM',2,4000.0,70.0),
 (16,'PL-5E II WVRAAM',2,3700.0,35.0),
 (17,'Burraq UCAV',3,463.0,500.0),
 (18,'Shahpar-I MALE UAV',3,180.0,750.0),
 (19,'Shahpar-II MALE UAV',3,220.0,1000.0),
 (20,'Uqab Tactical UAV',3,150.0,150.0),
 (21,'Jasoos Mini-UAV',3,80.0,50.0);");
    }
}
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapHub<RadarHub>("/radarHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();