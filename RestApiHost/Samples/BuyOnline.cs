using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestApiHost;

namespace BuyOnline
{
    public static partial class I
    {
        static Load<CartId, Cart> InMemoryLookup(IReadOnlyDictionary<CartId, Cart> dic) =>
            keys => Task.FromResult<IDictionary<CartId, Cart>>(keys
                .Select(a => (Exists: dic.TryGetValue(a, out var v), Id: a, Value: v))
                .Where(a => a.Exists)
                .ToDictionary(a => a.Id, a => a.Value));

        static Task<IDictionary<CartId, Cart>> FoldCartEvents(
            IEnumerable<IDeserializable> list,
            Load<CartId, Cart> load) =>
            RestApiHost.I.Fold<CartId, OneOf<ItemsAdded, ItemsRemoved, CartAssigned, Unit>, Cart>(
                list,
                load,
                a => a.OneOf<ItemsAdded, ItemsRemoved, CartAssigned>(),
                Cart.TryParse,
                Cart.Map);
    }

    public class Cart
    {
        public static bool TryLoad(Record record, out Cart cart, out List<string> errors)
        {
            errors = new List<string>();
            cart = record;
            return !errors.Any();
        }

        public static implicit operator Cart(Record record) =>
          new Cart(record.OrderId, record.Skus, record.CustomerId);

        public static bool TryParse(
          OneOf<ItemsAdded, ItemsRemoved, CartAssigned, Unit> oneOf,
          out CartId id)
        {
            var matched = true;

            id = oneOf.Match(
              a => Id(a.OrderId),
              b => Id(b.OrderId),
              c => Id(c.OrderId),
              _ => { matched = false; return null; });

            return matched;
        }

        public static Cart Map(
          OneOf<ItemsAdded, ItemsRemoved, CartAssigned, Unit> oneOf,
          Cart cart) =>
          oneOf.Match(
            a => cart.Add(a.Skus),
            a => cart.Remove(a.Skus),
            a => From(a),
            _ => cart);

        public T As<T>(Func<Record, T> map) =>
          map(new Record { OrderId = orderId, Skus = skus, CustomerId = customerId });

        public class Record
        {
            public string OrderId;
            public string[] Skus;
            public string CustomerId;
        }

        static CartId Id(string orderId) => new CartId { OrderId = orderId };

        static Cart From(CartAssigned a) =>
          new Cart(a.OrderId, new string[] { }, a.CustomerId);

        Cart Add(IReadOnlyList<string> skus) => new Cart(
          this.orderId,
          this.skus.Concat(skus).ToArray(),
          this.customerId);

        Cart Remove(IReadOnlyList<string> skus)
        {
            var a = this.skus.GroupBy(x => x)
              .ToDictionary(x => x.Key, x => x.Select(y => y).Count());

            var b = skus.GroupBy(x => x)
              .ToDictionary(x => x.Key, x => x.Select(y => y).Count() * -1);

            var finalSkus = new[] { a, b }.SelectMany(y => y)
              .GroupBy(y => y.Key)
              .ToDictionary(y => y.Key, x => x.Sum(i => i.Value))
              .Where(y => y.Value > 0)
              .SelectMany(y => Enumerable.Range(0, y.Value).Select(e => y.Key))
              .ToArray();

            return new Cart(this.orderId, finalSkus, this.customerId);
        }

        private Cart(string orderId, string[] skus, string customerId)
        {
            this.orderId = orderId;
            this.skus = skus ?? new string[] { };
            this.customerId = customerId;
        }

        private readonly string orderId;
        private readonly string[] skus;
        private readonly string customerId;

        public override string ToString() =>
          new
          {
              orderId,
              skus = new[] { skus, new[] { "" } }
              .SelectMany(a => a)
              .Aggregate((x, y) => x + "+" + y),
              customerId
          }.ToString();
    }

    public class CartId : IEquatable<CartId>
    {
        public string OrderId { get; set; }

        public bool Equals(CartId other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other as CartId, null)) return false;
            return ((CartId)other).OrderId.Equals(this.OrderId);
        }

        public override int GetHashCode() =>
          new { OrderId = this.OrderId }.GetHashCode();

        public override string ToString() =>
          new { OrderId = this.OrderId }.ToString();

        public static implicit operator CartId(string a) => new CartId { OrderId = a };
    }

    public class CartAssigned
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
    }

    public class ItemsAdded
    {
        public string OrderId { get; set; }
        public string[] Skus { get; set; }
    }

    public class ItemsRemoved
    {
        public string OrderId { get; set; }
        public string[] Skus { get; set; }
    }
}