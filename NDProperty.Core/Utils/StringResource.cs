using System.Globalization;

namespace NDProperty.Utils
{
    public abstract class StringResource
    {
        public static implicit operator string(StringResource resource) => resource.ToString(System.Globalization.CultureInfo.CurrentUICulture);
        public static implicit operator StringResource(string resource) => new SimpleResource(resource);

        protected abstract string ToString(CultureInfo currentUICulture);

        private class SimpleResource : StringResource
        {
            private readonly string str;

            public SimpleResource(string str)
            {
                this.str = str;
            }
            protected override string ToString(CultureInfo currentUICulture) => str;
        }
    }


}
