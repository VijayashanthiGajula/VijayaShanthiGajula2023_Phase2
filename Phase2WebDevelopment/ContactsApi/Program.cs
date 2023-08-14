using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;


var builder = WebApplication.CreateBuilder(args);
//adding connection string
var connectionString = builder.Configuration.GetConnectionString("contacts") ?? "Data Source=Contacts.db";
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddDbContext<ContactsDb>(options => options.UseInMemoryDatabase("items"));
builder.Services.AddDbContext<ContactsDb>(options => options.UseSqlite(connectionString));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Contacts API",
     Description = "Save your contacts", Version = "v1" });
});
//authentication and authorization services 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtConfig =>
    {
        jwtConfig.Authority = "https://example.com";
        jwtConfig.TokenValidationParameters = new()
        {
            ValidAudience = "MyAudience",
            ValidIssuer = "https://example.com"
        };
    });
builder.Services.AddAuthorization();


var app = builder.Build();
//middleware 
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
   c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contacts API V1");
});
////get
//app.MapGet("/Contact", () =>  new {Name="Vijaya Shanthi", Email="vj@gmail.com" ,
// Contact="025252555", Location="AK"});
//get list
app.MapGet("/Contacts", async (ContactsDb db) => await db.Contacts.ToListAsync());
//app.MapGet("secured-route", () => "Hello, you are authorized to see this!")
//    .RequireAuthorization();
//get by id
app.MapGet("/Contact/{id}",   async (ContactsDb db, int id) => await db.Contacts.FindAsync(id));

//post
app.MapPost("/Contacts",   async (ContactsDb db, ContactItem cn) =>
{
    await db.Contacts.AddAsync(cn);
    await db.SaveChangesAsync();
    return Results.Created($"/Contact/{cn.Id}", cn);
});

//update an item
app.MapPut("/Contacts/{id}",   async (ContactsDb db, ContactItem updateContact, int id) =>
{
    var contact = await db.Contacts.FindAsync(id);

    if (contact is null) return Results.NotFound();

    contact.Name = updateContact.Name;
    contact.Email = updateContact.Email;
    contact.Contact = updateContact.Contact;
    contact.Location = updateContact.Location;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

//delete
app.MapDelete("/Contacts/{id}", [Authorize] async (ContactsDb db, int id) =>
{
    var contact = await db.Contacts.FindAsync(id);

    if (contact is null) return Results.NotFound();

    db.Contacts.Remove(contact);
    await db.SaveChangesAsync();

    return Results.NoContent();
});



app.Run();
class ContactItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string?  Contact { get; set; }
    public string? Location { get; set; }

}
class ContactsDb : DbContext
{
    public ContactsDb(DbContextOptions options) : base(options) { }
    public DbSet<ContactItem> Contacts { get; set; }

     
}

