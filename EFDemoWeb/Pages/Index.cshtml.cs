using EFDataAccessLibrary.DataAccess;
using EFDataAccessLibrary.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

// Benefits of Entity Framework Core
// 1. Faster development speed
// 2. You don't have to know SQL

// Benefits of Dapper
// 1. Faster in production
// 2. Easier to work with for SQL developer
// 3. Designed for loose coupling

namespace EFDemoWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly PeopleContext _db;

        public IndexModel(ILogger<IndexModel> logger, PeopleContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task OnGet()
        {
            await LoadSampleData();

            var people = await _db.People
                .Include(a => a.Addresses)
                .Include(b => b.EmailAddresses)
                // It cant translate C# code to T-sql to execute it on sql server
                // so it downloads whole result before this clause 
                // and client side C# removes the unnecessary records
                //.Where(x => ApprovedAge(x.Age))
                .Where(x => x.Age >= 18 && x.Age <= 65)
                .ToListAsync();
        }

        private bool ApprovedAge(int age)
        {
            return age >= 18 && age <= 65;
        }

        private async Task LoadSampleData()
        {
            if(!_db.People.Any())
            {
                // https://json-generator.com/
                var file = await System.IO.File.ReadAllTextAsync("generated.json");
                var people = JsonSerializer.Deserialize<List<Person>>(file);

                if (!people.Any())
                {
                    return;
                }

                await _db.People.AddRangeAsync(people);
                await _db.SaveChangesAsync();
            }
        }
    }
}