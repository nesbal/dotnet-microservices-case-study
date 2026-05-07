using Microsoft.AspNetCore.Authorization;

namespace ProductService.Authorization;

public class AdminOrOwnerRequirement : IAuthorizationRequirement
{
}