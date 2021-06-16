using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestApiHost
{
    public delegate Task<IDictionary<TId, TDoc>> Load<TId, TDoc>(TId[] ids);

    public delegate bool Parse<A, B>(A a, out B b);
    public delegate B Map<A, B>(A a, B b);

    public static partial class I
    {
        public async static Task<IDictionary<TId, TState>> Fold<TId, TInput, TState>(
            IEnumerable<IDeserializable> list,
            Load<TId, TState> load,
            Func<IDeserializable, TInput> asTypedInput,
            Parse<TInput, TId> idFromInput,
            Map<TInput, TState> foldInputs)
        {
            var inputsById = list
              .Select(asTypedInput)
              .Select(a => new { Parsed = idFromInput(a, out TId id), Id = id, TypedInput = a })
              .Where(a => a.Parsed)
              .Select(a => (a.Id, a.TypedInput))
              .GroupBy(a => a.Id)
              .ToDictionary(a => a.Key, a => a.Select(b => b.TypedInput));

            var stateById = await load(inputsById.Keys.ToArray());

            var statePlusInputsById = inputsById
              .Select(a => (Id: a.Key, State: stateById[a.Key], TypedInputs: a.Value))
              .ToDictionary(a => a.Id, a => (a.State, a.TypedInputs));

            var resultsById = new Dictionary<TId, TState>();

            foreach (var statePlusInputs in statePlusInputsById)
            {
                var state = statePlusInputs.Value.State;
                foreach (var ev in statePlusInputs.Value.TypedInputs)
                    state = foldInputs(ev, state);
                resultsById[statePlusInputs.Key] = state;
            }

            return resultsById;
        }
    }

    public interface IDeserializable
    {
        bool As<T>(out T value) where T : class;
    }

    static class Parsers
    {
        public static JsonDoc AsJsonDoc<T>(this T instance, long offset) where T : class =>
          new JsonDoc(
            jsonPayload: JsonSerializer.Serialize(instance),
            offset,
            instance.GetType().FullName);

        public static OneOf<A, B, Unit> OneOf<A, B>(
          this IDeserializable canBeParsed)
          where A : class
          where B : class
        {
            if (canBeParsed.As<A>(out var a)) return a;
            if (canBeParsed.As<B>(out var b)) return b;
            return new Unit();
        }

        public static OneOf<A, B, C, Unit> OneOf<A, B, C>(
          this IDeserializable canBeParsed)
          where A : class
          where B : class
          where C : class
        {
            if (canBeParsed.As<A>(out var a)) return a;
            if (canBeParsed.As<B>(out var b)) return b;
            if (canBeParsed.As<C>(out var c)) return c;
            return new Unit();
        }

        public static OneOf<A, B, C, D, Unit> OneOf<A, B, C, D>(
          this IDeserializable canBeParsed)
          where A : class
          where B : class
          where C : class
          where D : class
        {
            if (canBeParsed.As<A>(out var a)) return a;
            if (canBeParsed.As<B>(out var b)) return b;
            if (canBeParsed.As<C>(out var c)) return c;
            if (canBeParsed.As<D>(out var d)) return d;
            return new Unit();
        }
    }

    class JsonDoc : IDeserializable, IComparable<JsonDoc>
    {
        private readonly string jsonPayload;
        private readonly long offset;
        private readonly string uniqueTypeName;

        public JsonDoc(
          string jsonPayload,
          long offset,
          string uniqueTypeName)
        {
            this.jsonPayload = jsonPayload;
            this.offset = offset;
            this.uniqueTypeName = uniqueTypeName;
        }

        public bool As<T>(out T value) where T : class
        {
            value = null;
            if (typeof(T).FullName.Equals(
              uniqueTypeName,
              StringComparison.OrdinalIgnoreCase))
            {
                value = (T)JsonSerializer.Deserialize(jsonPayload, typeof(T));
                return true;
            }
            return false;
        }

        public int CompareTo(JsonDoc other) =>
          this.offset.CompareTo(other.offset);

        public override string ToString() =>
          new { jsonPayload, offset, uniqueTypeName }.ToString();
    }
}