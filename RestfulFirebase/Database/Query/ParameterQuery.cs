using System;

namespace RestfulFirebase.Database.Query
{
    public abstract class ParameterQuery : FirebaseQuery
    {
        private readonly Func<string> parameterFactory;
        private readonly string separator;

        protected ParameterQuery(FirebaseQuery parent, Func<string> parameterFactory, RestfulFirebaseApp app)
            : base(parent, app)
        {
            this.parameterFactory = parameterFactory;
            separator = (Parent is ChildQuery) ? "?" : "&";
        }

        protected override string BuildUrlSegment(FirebaseQuery child)
        {
            return $"{separator}{parameterFactory()}={BuildUrlParameter(child)}";
        }

        protected abstract string BuildUrlParameter(FirebaseQuery child);

        public FilterQuery StartAt(Func<string> valueFactory)
        {
            return new FilterQuery(this, () => "startAt", valueFactory, App);
        }

        public FilterQuery EndAt(Func<string> valueFactory)
        {
            return new FilterQuery(this, () => "endAt", valueFactory, App);
        }

        public FilterQuery EqualTo(Func<string> valueFactory)
        {
            return new FilterQuery(this, () => "equalTo", valueFactory, App);
        }

        public FilterQuery StartAt(Func<double> valueFactory)
        {
            return new FilterQuery(this, () => "startAt", valueFactory, App);
        }

        public FilterQuery EndAt(Func<double> valueFactory)
        {
            return new FilterQuery(this, () => "endAt", valueFactory, App);
        }

        public FilterQuery EqualTo(Func<double> valueFactory)
        {
            return new FilterQuery(this, () => "equalTo", valueFactory, App);
        }

        public FilterQuery StartAt(Func<long> valueFactory)
        {
            return new FilterQuery(this, () => "startAt", valueFactory, App);
        }

        public FilterQuery EndAt(Func<long> valueFactory)
        {
            return new FilterQuery(this, () => "endAt", valueFactory, App);
        }

        public FilterQuery EqualTo(Func<long> valueFactory)
        {
            return new FilterQuery(this, () => "equalTo", valueFactory, App);
        }

        public FilterQuery EqualTo(Func<bool> valueFactory)
        {
            return new FilterQuery(this, () => "equalTo", valueFactory, App);
        }

        public FilterQuery LimitToFirst(Func<int> countFactory)
        {
            return new FilterQuery(this, () => "limitToFirst", () => countFactory(), App);
        }

        public FilterQuery LimitToLast(Func<int> countFactory)
        {
            return new FilterQuery(this, () => "limitToLast", () => countFactory(), App);
        }

        public FilterQuery StartAt(string value)
        {
            return StartAt(() => value);
        }

        public FilterQuery EndAt(string value)
        {
            return EndAt(() => value);
        }

        public FilterQuery EqualTo(string value)
        {
            return EqualTo(() => value);
        }

        public FilterQuery StartAt(double value)
        {
            return StartAt(() => value);
        }

        public FilterQuery EndAt(double value)
        {
            return EndAt(() => value);
        }

        public FilterQuery EqualTo(double value)
        {
            return EqualTo(() => value);
        }

        public FilterQuery StartAt(long value)
        {
            return StartAt(() => value);
        }

        public FilterQuery EndAt(long value)
        {
            return EndAt(() => value);
        }

        public FilterQuery EqualTo(long value)
        {
            return EqualTo(() => value);
        }

        public FilterQuery EqualTo(bool value)
        {
            return EqualTo(() => value);
        }

        public FilterQuery EqualTo()
        {
            return EqualTo(() => null);
        }

        public FilterQuery LimitToFirst(int count)
        {
            return LimitToFirst(() => count);
        }

        public FilterQuery LimitToLast(int count)
        {
            return LimitToLast(() => count);
        }
    }
}
