using System;

namespace RestApiHost
{
    public struct Unit { }

    public class OneOf<TA, TB>
    {
        private readonly TA _a;
        private readonly TB _b;
        private readonly bool _isB;

        public OneOf(TA left)
        {
            _a = left;
            _isB = false;
        }

        public OneOf(TB right)
        {
            _b = right;
            _isB = true;
        }

        public TResult Match<TResult>(Func<TA, TResult> fa, Func<TB, TResult> fb) =>
            _isB ? fb(_b) : fa(_a);

        public static implicit operator OneOf<TA, TB>(TA a) => new OneOf<TA, TB>(a);
        public static implicit operator OneOf<TA, TB>(TB b) => new OneOf<TA, TB>(b);
    }

    public class OneOf<TA, TB, TC>
    {
        private readonly OneOf<OneOf<TA, TB>, TC> _value;

        public OneOf(TA a) => _value = new OneOf<TA, TB>(a);
        public OneOf(TB b) => _value = new OneOf<TA, TB>(b);
        public OneOf(TC c) => _value = c;

        public T Match<T>(Func<TA, T> fa, Func<TB, T> fb, Func<TC, T> fc)
            => _value.Match(oneOf => oneOf.Match(fa, fb), fc);

        public static implicit operator OneOf<TA, TB, TC>(TA a) => new OneOf<TA, TB, TC>(a);
        public static implicit operator OneOf<TA, TB, TC>(TB b) => new OneOf<TA, TB, TC>(b);
        public static implicit operator OneOf<TA, TB, TC>(TC c) => new OneOf<TA, TB, TC>(c);
    }

    public class OneOf<TA, TB, TC, TD>
    {
        private readonly OneOf<OneOf<TA, TB, TC>, TD> _value;

        public OneOf(TA a) => _value = new OneOf<TA, TB, TC>(a);
        public OneOf(TB b) => _value = new OneOf<TA, TB, TC>(b);
        public OneOf(TC c) => _value = new OneOf<TA, TB, TC>(c);
        public OneOf(TD d) => _value = d;

        public T Match<T>(Func<TA, T> fa, Func<TB, T> fb, Func<TC, T> fc, Func<TD, T> fd)
            => _value.Match(oneOf => oneOf.Match(fa, fb, fc), fd);

        public static implicit operator OneOf<TA, TB, TC, TD>(TA a) => new OneOf<TA, TB, TC, TD>(a);
        public static implicit operator OneOf<TA, TB, TC, TD>(TB b) => new OneOf<TA, TB, TC, TD>(b);
        public static implicit operator OneOf<TA, TB, TC, TD>(TC c) => new OneOf<TA, TB, TC, TD>(c);
        public static implicit operator OneOf<TA, TB, TC, TD>(TD d) => new OneOf<TA, TB, TC, TD>(d);
    }

    public class OneOf<TA, TB, TC, TD, TE>
    {
        private readonly OneOf<OneOf<TA, TB, TC, TD>, TE> _value;

        public OneOf(TA a) => _value = new OneOf<TA, TB, TC, TD>(a);
        public OneOf(TB b) => _value = new OneOf<TA, TB, TC, TD>(b);
        public OneOf(TC c) => _value = new OneOf<TA, TB, TC, TD>(c);
        public OneOf(TD d) => _value = new OneOf<TA, TB, TC, TD>(d);
        public OneOf(TE e) => _value = e;

        public T Match<T>(Func<TA, T> fa, Func<TB, T> fb, Func<TC, T> fc, Func<TD, T> fd, Func<TE, T> fe)
            => _value.Match(oneOf => oneOf.Match(fa, fb, fc, fd), fe);

        public static implicit operator OneOf<TA, TB, TC, TD, TE>(TA a) => new OneOf<TA, TB, TC, TD, TE>(a);
        public static implicit operator OneOf<TA, TB, TC, TD, TE>(TB b) => new OneOf<TA, TB, TC, TD, TE>(b);
        public static implicit operator OneOf<TA, TB, TC, TD, TE>(TC c) => new OneOf<TA, TB, TC, TD, TE>(c);
        public static implicit operator OneOf<TA, TB, TC, TD, TE>(TD d) => new OneOf<TA, TB, TC, TD, TE>(d);
        public static implicit operator OneOf<TA, TB, TC, TD, TE>(TE e) => new OneOf<TA, TB, TC, TD, TE>(e);
    }
}