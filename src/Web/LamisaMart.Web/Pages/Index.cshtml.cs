using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LamisaMart.PageBuilder.Application.Layouts.Queries;

namespace LamisaMart.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IMediator _mediator;

    public PageLayoutDto? PageLayout { get; set; }

    public IndexModel(ILogger<IndexModel> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task OnGetAsync()
    {
        // Try to fetch the highly customizable home page layout (Route = "/")
        PageLayout = await _mediator.Send(new GetPageLayoutQuery("/"));
        
        // If not found, it falls back to hardcoded HTML in the Index.cshtml
    }
}
