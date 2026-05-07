using Microsoft.AspNetCore.Authorization;
using ProductService.Application.Interfaces;

namespace ProductService.Authorization;

public class AdminOrOwnerHandler : AuthorizationHandler<AdminOrOwnerRequirement>
{
    private readonly IProductRepository _productRepository;

    public AdminOrOwnerHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminOrOwnerRequirement requirement)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is not HttpContext httpContext)
        {
            return;
        }

        var routeId = httpContext.Request.RouteValues["id"]?.ToString();

        if (!int.TryParse(routeId, out var productId))
        {
            return;
        }

        var product = await _productRepository.GetByIdAsync(productId);

        if (product?.OwnerUsername == context.User.Identity?.Name)
        {
            context.Succeed(requirement);
        }
    }
}