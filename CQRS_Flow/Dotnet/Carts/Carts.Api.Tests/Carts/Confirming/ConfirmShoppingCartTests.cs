using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Carts.Api.Requests.Carts;
using Carts.Carts;
using Carts.Carts.GettingCartById;
using Core.Testing;
using FluentAssertions;
using Xunit;

namespace Carts.Api.Tests.Carts.Confirming;

public class ConfirmShoppingCartFixture: ApiFixture<Startup>
{
    protected override string ApiUrl => "/api/Carts";

    public Guid CartId { get; private set; }

    public readonly Guid ClientId = Guid.NewGuid();

    public HttpResponseMessage CommandResponse = default!;

    public override async Task InitializeAsync()
    {
        var initializeResponse = await Post(new InitializeCartRequest(ClientId));
        initializeResponse.EnsureSuccessStatusCode();

        CartId = await initializeResponse.GetResultFromJson<Guid>();

        CommandResponse = await Put($"{CartId}/confirmation");
    }
}

public class ConfirmShoppingCartTests: IClassFixture<ConfirmShoppingCartFixture>
{
    private readonly ConfirmShoppingCartFixture fixture;

    public ConfirmShoppingCartTests(ConfirmShoppingCartFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public Task Put_Should_Return_OK()
    {
        var commandResponse = fixture.CommandResponse.EnsureSuccessStatusCode();
        commandResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        return Task.CompletedTask;
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task Put_Should_Confirm_ShoppingCart()
    {
        // prepare query
        var query = $"{fixture.CartId}";

        //send query
        var queryResponse = await fixture.Get(query, 30,
            check: async response => (await response.GetResultFromJson<CartDetails>()).Version == 1);

        queryResponse.EnsureSuccessStatusCode();

        var cartDetails = await queryResponse.GetResultFromJson<CartDetails>();
        cartDetails.Should().NotBeNull();
        cartDetails.Version.Should().Be(1);
        cartDetails.Id.Should().Be(fixture.CartId);
        cartDetails.Status.Should().Be(CartStatus.Confirmed);
        cartDetails.ClientId.Should().Be(fixture.ClientId);
    }
}
