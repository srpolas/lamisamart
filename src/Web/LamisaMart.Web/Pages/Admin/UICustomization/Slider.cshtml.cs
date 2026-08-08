using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LamisaMart.Web.Pages.Admin.UICustomization;

[Authorize(Roles = "SuperAdmin,SuperUser,Admin")]
public class SliderModel : PageModel
{
    public static List<HeroSlideItem> SlidesStore { get; set; } = InitDefaultSlides();

    public class HeroSlideItem
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string BadgeText { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string ButtonText { get; set; } = string.Empty;
        public string ButtonLink { get; set; } = string.Empty;
        public string TextAlignment { get; set; } = "left"; // left, center, right
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public List<HeroSlideItem> SlidesList { get; set; } = new();

    public void OnGet()
    {
        SlidesList = SlidesStore.OrderBy(s => s.SortOrder).ToList();
    }

    public IActionResult OnPostAddSlide(
        string imageUrl,
        string badgeText,
        string title,
        string subtitle,
        string buttonText,
        string buttonLink,
        string textAlignment,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || string.IsNullOrWhiteSpace(title))
        {
            TempData["ErrorMessage"] = "Slider image URL and slide title are required.";
            return RedirectToPage();
        }

        var newSlide = new HeroSlideItem
        {
            Id = Guid.NewGuid(),
            ImageUrl = imageUrl.Trim(),
            BadgeText = badgeText?.Trim() ?? "",
            Title = title.Trim(),
            Subtitle = subtitle?.Trim() ?? "",
            ButtonText = string.IsNullOrWhiteSpace(buttonText) ? "SHOP NOW" : buttonText.Trim(),
            ButtonLink = string.IsNullOrWhiteSpace(buttonLink) ? "/category/saree" : buttonLink.Trim(),
            TextAlignment = textAlignment ?? "left",
            SortOrder = sortOrder > 0 ? sortOrder : SlidesStore.Count + 1,
            IsActive = true
        };

        SlidesStore.Add(newSlide);
        TempData["SuccessMessage"] = $"New Hero Slider slide '{newSlide.Title}' added successfully!";
        return RedirectToPage();
    }

    public IActionResult OnPostEditSlide(
        Guid id,
        string imageUrl,
        string badgeText,
        string title,
        string subtitle,
        string buttonText,
        string buttonLink,
        string textAlignment,
        int sortOrder,
        bool isActive)
    {
        var slide = SlidesStore.FirstOrDefault(s => s.Id == id);
        if (slide != null)
        {
            slide.ImageUrl = imageUrl.Trim();
            slide.BadgeText = badgeText?.Trim() ?? "";
            slide.Title = title.Trim();
            slide.Subtitle = subtitle?.Trim() ?? "";
            slide.ButtonText = buttonText?.Trim() ?? "SHOP NOW";
            slide.ButtonLink = buttonLink?.Trim() ?? "/category/saree";
            slide.TextAlignment = textAlignment ?? "left";
            slide.SortOrder = sortOrder;
            slide.IsActive = isActive;

            TempData["SuccessMessage"] = $"Slide '{slide.Title}' updated successfully!";
        }
        return RedirectToPage();
    }

    public IActionResult OnPostToggleSlide(Guid id)
    {
        var slide = SlidesStore.FirstOrDefault(s => s.Id == id);
        if (slide != null)
        {
            slide.IsActive = !slide.IsActive;
            TempData["SuccessMessage"] = $"Slide '{slide.Title}' status changed to {(slide.IsActive ? "Active" : "Inactive")}!";
        }
        return RedirectToPage();
    }

    public IActionResult OnPostDeleteSlide(Guid id)
    {
        var slide = SlidesStore.FirstOrDefault(s => s.Id == id);
        if (slide != null)
        {
            SlidesStore.Remove(slide);
            TempData["SuccessMessage"] = $"Slide '{slide.Title}' deleted successfully!";
        }
        return RedirectToPage();
    }

    private static List<HeroSlideItem> InitDefaultSlides()
    {
        return new List<HeroSlideItem>
        {
            new()
            {
                Id = Guid.Parse("aa111111-1111-1111-1111-111111111111"),
                ImageUrl = "/images/LamisaMart_design_concept.png",
                BadgeText = "✨ Exclusive Eid & Festive Collection 2026",
                Title = "প্রতিদিনের সাজে, লমিসা মার্ট।",
                Subtitle = "Everyday Elegance, Authentic Bangladeshi Craftsmanship.",
                ButtonText = "SHOP SAREES & WEAVES",
                ButtonLink = "/category/saree",
                TextAlignment = "left",
                SortOrder = 1,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("aa222222-2222-2222-2222-222222222222"),
                ImageUrl = "https://images.unsplash.com/photo-1610030469983-98e550d6193c?w=1200&q=80",
                BadgeText = "🌿 Handwoven Artisanal Heritage",
                Title = "Dhakai Jamdani & Rajshahi Katan Silk",
                Subtitle = "Direct from native Narayanganj and Rajshahi master weavers.",
                ButtonText = "EXPLORE JAMDANI",
                ButtonLink = "/category/saree",
                TextAlignment = "left",
                SortOrder = 2,
                IsActive = true
            },
            new()
            {
                Id = Guid.Parse("aa333333-3333-3333-3333-333333333333"),
                ImageUrl = "https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=1200&q=80",
                BadgeText = "🔥 New Arrival Designer Collection",
                Title = "Luxury Three-Piece Suits & Designer Kurtis",
                Subtitle = "Premium cotton, organza, and Georgette embroidered sets.",
                ButtonText = "SHOP THREE PIECE",
                ButtonLink = "/category/three-piece",
                TextAlignment = "left",
                SortOrder = 3,
                IsActive = true
            }
        };
    }
}
