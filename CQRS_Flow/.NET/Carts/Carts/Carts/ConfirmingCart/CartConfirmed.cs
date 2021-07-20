using System;
using Core.Events;

namespace Carts.Carts.ConfirmingCart
{
    public class CartConfirmed: IEvent
    {
        public Guid CartId { get; }

        public DateTime ConfirmedAt { get; }

        public CartConfirmed(Guid cartId, DateTime confirmedAt)
        {
            CartId = cartId;
            ConfirmedAt = confirmedAt;
        }

        public static CartConfirmed Create(Guid cartId, DateTime confirmedAt)
        {
            if (cartId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(cartId));

            if(confirmedAt == DateTime.MinValue || confirmedAt == DateTime.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(confirmedAt));

            return new CartConfirmed(cartId, confirmedAt);
        }
    }
}
