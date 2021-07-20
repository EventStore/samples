using System;

namespace Carts.Api.Requests.Carts
{
    public record InitializeCartRequest(
        Guid? ClientId
    );
}
