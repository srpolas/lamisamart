using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace LamisaMart.Web.Pages.Admin.Products;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class TagsModel : PageModel
{
    private readonly ILogger<TagsModel> _logger;

    public TagsModel(ILogger<TagsModel> logger)
    {
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public List<TagViewModel> TagsList { get; set; } = new();

    public class TagViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public void OnGet()
    {
        TagsList = GetSampleTags(SearchQuery);
    }

    public IActionResult OnPostCreateTag(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            TempData["ErrorMessage"] = "Tag name is required.";
            return RedirectToPage();
        }

        TempData["SuccessMessage"] = $"Tag '#{tagName.Trim()}' created successfully!";
        return RedirectToPage();
    }

    public IActionResult OnPostDelete(Guid tagId)
    {
        TempData["SuccessMessage"] = "Tag deleted successfully.";
        return RedirectToPage();
    }

    private List<TagViewModel> GetSampleTags(string? search)
    {
        var list = new List<TagViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Jamdani", Slug = "jamdani", ProductCount = 124, CreatedAt = DateTime.UtcNow.AddMonths(-5) },
            new() { Id = Guid.NewGuid(), Name = "Handloom", Slug = "handloom", ProductCount = 98, CreatedAt = DateTime.UtcNow.AddMonths(-4) },
            new() { Id = Guid.NewGuid(), Name = "Silk", Slug = "silk", ProductCount = 85, CreatedAt = DateTime.UtcNow.AddMonths(-4) },
            new() { Id = Guid.NewGuid(), Name = "Eid2026", Slug = "eid2026", ProductCount = 140, CreatedAt = DateTime.UtcNow.AddMonths(-2) },
            new() { Id = Guid.NewGuid(), Name = "PujaCollection", Slug = "puja-collection", ProductCount = 76, CreatedAt = DateTime.UtcNow.AddMonths(-1) },
            new() { Id = Guid.NewGuid(), Name = "GoldZari", Slug = "gold-zari", ProductCount = 62, CreatedAt = DateTime.UtcNow.AddMonths(-1) }
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            list = list.Where(t => t.Name.ToLower().Contains(q) || t.Slug.ToLower().Contains(q)).ToList();
        }

        return list;
    }
}
