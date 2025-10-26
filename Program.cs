using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using BusinessLogic.Services; // <-- Цей using у вас вже є

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- ЗМІНЕНО ТУТ: Додано .AddRoles<IdentityRole>() ---
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // <-- Вмикаємо підтримку ролей
    .AddEntityFrameworkStores<ApplicationDbContext>();
// -----------------------------------------------------

builder.Services.AddControllersWithViews();

// Register business services
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<SportService>();

var app = builder.Build();

// --- ДОДАНО КОД ДЛЯ СТВОРЕННЯ РОЛЕЙ ТА ПРИЗНАЧЕННЯ АДМІНА ---
using (var scope = app.Services.CreateAsyncScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // Назви ролей
        string adminRole = "Admin";
        string userRole = "User";

        // Створюємо роль Admin, якщо її ще немає
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
            Console.WriteLine($"Role '{adminRole}' created."); // Логування
        }
        else
        {
            Console.WriteLine($"Role '{adminRole}' already exists."); // Логування
        }


        // Створюємо роль User, якщо її ще немає
        if (!await roleManager.RoleExistsAsync(userRole))
        {
            await roleManager.CreateAsync(new IdentityRole(userRole));
            Console.WriteLine($"Role '{userRole}' created."); // Логування
        }
        else
        {
            Console.WriteLine($"Role '{userRole}' already exists."); // Логування
        }

        // --- Призначення ролі Admin конкретному користувачу ---
        // ЗАМІНІТЬ ЦЕЙ EMAIL НА ВАШ EMAIL
        string adminEmail = "solimchuk_ak24@nuwm.edu.ua"; // <-- Перевірте, чи це ваш email!
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser != null)
        {
            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                var result = await userManager.AddToRoleAsync(adminUser, adminRole); // Зберігаємо результат
                if (result.Succeeded)
                {
                    Console.WriteLine($"Role '{adminRole}' assigned to user '{adminEmail}'."); // Логування успіху
                }
                else
                {
                    Console.WriteLine($"Failed to assign role '{adminRole}' to user '{adminEmail}'. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}"); // Логування помилки
                }
            }
            else
            {
                Console.WriteLine($"User '{adminEmail}' already has role '{adminRole}'."); // Логування
            }
        }
        else
        {
            Console.WriteLine($"Admin user with email '{adminEmail}' not found. Cannot assign role."); // Логування
        }
    }
    catch (Exception ex)
    {
        // Логуємо помилку, якщо щось пішло не так під час створення ролей
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database with roles and admin user.");
    }
}
// --- КІНЕЦЬ КОДУ ДЛЯ СТВОРЕННЯ РОЛЕЙ ---


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
app.UseStaticFiles(); // Переконайтесь, що цей рядок є
app.UseRouting();

// --- ПОРЯДОК ВАЖЛИВИЙ ---
app.UseAuthentication(); // Має бути перед UseAuthorization
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

