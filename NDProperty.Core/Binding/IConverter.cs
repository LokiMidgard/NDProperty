namespace NDProperty.Binding
{
    public interface IConverter<TSource, TDestination>
    {
        TDestination ConvertTo(TSource source);
    }
    public interface ITwoWayConverter<TSource, TDestination> :IConverter<TSource, TDestination>
    {
        TSource ConvertTo(TDestination source);
    }
}