using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using BusinessLogic.Services; // <-- ��� using � ��� ��� �

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- �̲���� ���: ������ .AddRoles<IdentityRole>() ---
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // <-- ������� �������� �����
    .AddEntityFrameworkStores<ApplicationDbContext>();
// -----------------------------------------------------

builder.Services.AddControllersWithViews();

// Register business services
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<SportService>();

var app = builder.Build();

// --- ������ ��� ��� ��������� ����� �� ����������� ��̲�� ---
using (var scope = app.Services.CreateAsyncScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // ����� �����
        string adminRole = "Admin";
        string userRole = "User";

        // ��������� ���� Admin, ���� �� �� ����
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
            Console.WriteLine($"Role '{adminRole}' created."); // ���������
        }
        else
        {
            Console.WriteLine($"Role '{adminRole}' already exists."); // ���������
        }


        // ��������� ���� User, ���� �� �� ����
        if (!await roleManager.RoleExistsAsync(userRole))
        {
            await roleManager.CreateAsync(new IdentityRole(userRole));
            Console.WriteLine($"Role '{userRole}' created."); // ���������
        }
        else
        {
            Console.WriteLine($"Role '{userRole}' already exists."); // ���������
        }

        // --- ����������� ��� Admin ����������� ����������� ---
        // ��̲Ͳ�� ��� EMAIL �� ��� EMAIL
        string adminEmail = "solimchuk_ak24@nuwm.edu.ua"; // <-- ��������, �� �� ��� email!
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser != null)
        {
            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                var result = await userManager.AddToRoleAsync(adminUser, adminRole); // �������� ���������
                if (result.Succeeded)
                {
                    Console.WriteLine($"Role '{adminRole}' assigned to user '{adminEmail}'."); // ��������� �����
                }
                else
                {
                    Console.WriteLine($"Failed to assign role '{adminRole}' to user '{adminEmail}'. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}"); // ��������� �������
                }
            }
            else
            {
                Console.WriteLine($"User '{adminEmail}' already has role '{adminRole}'."); // ���������
            }
        }
        else
        {
            Console.WriteLine($"Admin user with email '{adminEmail}' not found. Cannot assign role."); // ���������
        }
    }
    catch (Exception ex)
    {
        // ������ �������, ���� ���� ���� �� ��� �� ��� ��������� �����
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database with roles and admin user.");
    }
}
// --- ʲ���� ���� ��� ��������� ����� ---


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // �������������, �� ��� ����� �
app.UseRouting();

// --- ������� �������� ---
app.UseAuthentication(); // �� ���� ����� UseAuthorization
app.UseAuthorization();
// -------------------------

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();

