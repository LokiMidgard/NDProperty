using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

using NDProperty.Propertys;

namespace NDProperty.Binding
{
    /// <summary>
    /// This static class provides Methods to implement Binding.
    /// </summary>
    /// <remarks>
    /// Every Method in this class returns an object that implements <see cref="IDisposable"/>.
    /// To revoke the binding call the Dispose Method on the Object. The <see cref="IDisposable.Dispose"/>
    /// method will not be called when the object is garbage collected. If you don't want to untie the binding
    /// you need not to hold an reference to the Binding object. <para/>
    /// When binding two way to POCO, this implementation assumes that the <see cref="INotifyPropertyChanged.PropertyChanged"/> 
    /// event is fired only when the value actually changes. If this is not the case this can result in unwanted round trips.
    /// </remarks>
    public static class Bind
    {

        /// <summary>
        /// Creaates a One Way Binding using NDPropertys.
        /// </summary>
        /// <typeparam name="TValueSource">The type of the source Property</typeparam>
        /// <typeparam name="TTypeSource">The type of the source object</typeparam>
        /// <typeparam name="TValueDestination">The type of the destination Property</typeparam>
        /// <typeparam name="TTypeDestination">The type of the destination object</typeparam>
        /// <param name="source">The source Object</param>
        /// <param name="sourceProperty">The source Property</param>
        /// <param name="destination">The destination Object</param>
        /// <param name="destinationProperty">The destination Property</param>
        /// <param name="converter">A converter that is used to translate from source property type to destination proeprty type.</param>
        /// <returns>The binding object</returns>
        public static OneWayBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination> OneWay<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination>(TTypeSource source, NDReadOnlyPropertyKey<TKey, TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, NDPropertyKey<TKey, TValueDestination, TTypeDestination> destinationProperty, IConverter<TValueSource, TValueDestination> converter)
            where TTypeSource : class
            where TTypeDestination : class
        {
            return new OneWayBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination>(source, sourceProperty, destination, destinationProperty, converter);
        }
        /// <summary>
        /// Creaates a One Way Binding using NDPropertys.
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TTypeSource">The type of the source object</typeparam>
        /// <typeparam name="TTypeDestination">The type of the destination object</typeparam>
        /// <param name="source">The source Object</param>
        /// <param name="sourceProperty">The source Property</param>
        /// <param name="destination">The destination Object</param>
        /// <param name="destinationProperty">The destination Property</param>
        /// <returns>The binding object</returns>
        public static OneWayBinding<TKey, TValue, TTypeSource, TValue, TTypeDestination> OneWay<TKey, TValue, TTypeSource, TTypeDestination>(TTypeSource source, NDReadOnlyPropertyKey<TKey, TValue, TTypeSource> sourceProperty, TTypeDestination destination, NDPropertyKey<TKey, TValue, TTypeDestination> destinationProperty)
            where TTypeSource : class
            where TTypeDestination : class
        {
            return new OneWayBinding<TKey, TValue, TTypeSource, TValue, TTypeDestination>(source, sourceProperty, destination, destinationProperty, new IdentetyConverter<TValue>());
        }

        /// <summary>
        /// Creates one way binding from POCO to NDProperty
        /// </summary>
        /// <typeparam name="TValueSource">The type of the source Property</typeparam>
        /// <typeparam name="TTypeSource">The type of the source object</typeparam>
        /// <typeparam name="TValueDestination">The type of the destination Property</typeparam>
        /// <typeparam name="TTypeDestination">The type of the destination object</typeparam>
        /// <param name="source">The source Object</param>
        /// <param name="sourceProperty">The name of the source Property</param>
        /// <param name="destination">The destination Object</param>
        /// <param name="destinationProperty">The destination Property</param>
        /// <param name="converter">A converter that is used to translate from source property type to destination proeprty type.</param>
        /// <returns>The binding object</returns>
        public static NotifyToNDPBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination> OneWay<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination>(TTypeSource source, string sourceProperty, TTypeDestination destination, NDPropertyKey<TKey, TValueDestination, TTypeDestination> destinationProperty, IConverter<TValueSource, TValueDestination> converter)
            where TTypeSource : INotifyPropertyChanged
            where TTypeDestination : class
        {
            return new NotifyToNDPBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination>(source, sourceProperty, destination, destinationProperty, converter);
        }
        /// <summary>
        /// Creates one way binding from POCO to NDProperty
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TTypeSource">The type of the source object</typeparam>
        /// <typeparam name="TTypeDestination">The type of the destination object</typeparam>
        /// <param name="source">The source Object</param>
        /// <param name="sourceProperty">The name of the source Property</param>
        /// <param name="destination">The destination Object</param>
        /// <param name="destinationProperty">The destination Property</param>
        /// <returns>The binding object</returns>
        public static NotifyToNDPBinding<TKey, TValue, TTypeSource, TValue, TTypeDestination> OneWay<TKey, TValue, TTypeSource, TTypeDestination>(TTypeSource source, string sourceProperty, TTypeDestination destination, NDPropertyKey<TKey, TValue, TTypeDestination> destinationProperty)
            where TTypeSource : INotifyPropertyChanged
            where TTypeDestination : class
        {
            return new NotifyToNDPBinding<TKey, TValue, TTypeSource, TValue, TTypeDestination>(source, sourceProperty, destination, destinationProperty, new IdentetyConverter<TValue>());
        }
        /// <summary>
        /// Creates one way binding from NDProperty to POCO
        /// </summary>
        /// <typeparam name="TValueSource">The type of the source Property</typeparam>
        /// <typeparam name="TTypeSource">The type of the source object</typeparam>
        /// <typeparam name="TValueDestination">The type of the destination Property</typeparam>
        /// <typeparam name="TTypeDestination">The type of the destination object</typeparam>
        /// <param name="source">The source Object</param>
        /// <param name="sourceProperty">The source Property</param>
        /// <param name="destination">The destination Object</param>
        /// <param name="destinationProperty">The name of the destination Property</param>
        /// <param name="converter">A converter that is used to translate from source property type to destination proeprty type.</param>
        /// <returns>The binding object</returns>
        public static NDPToNotifyBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination> OneWay<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination>(TTypeSource source, NDReadOnlyPropertyKey<TKey, TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, string destinationProperty, IConverter<TValueSource, TValueDestination> converter)
            where TTypeSource : class
            where TTypeDestination : INotifyPropertyChanged
        {
            return new NDPToNotifyBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination>(source, sourceProperty, destination, destinationProperty, converter);
        }
        /// <summary>
        /// Creates one way binding from NDProperty to POCO
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TTypeSource">The type of the source object</typeparam>
        /// <typeparam name="TTypeDestination">The type of the destination object</typeparam>
        /// <param name="source">The source Object</param>
        /// <param name="sourceProperty">The source Property</param>
        /// <param name="destination">The name of the destination Object</param>
        /// <param name="destinationProperty">The destination Property</param>
        /// <returns>The binding object</returns>
        public static NDPToNotifyBinding<TKey, TValue, TTypeSource, TValue, TTypeDestination> OneWay<TKey, TValue, TTypeSource, TTypeDestination>(TTypeSource source, NDReadOnlyPropertyKey<TKey, TValue, TTypeSource> sourceProperty, TTypeDestination destination, string destinationProperty)
            where TTypeSource : class
            where TTypeDestination : INotifyPropertyChanged
        {
            return new NDPToNotifyBinding<TKey, TValue, TTypeSource, TValue, TTypeDestination>(source, sourceProperty, destination, destinationProperty, new IdentetyConverter<TValue>());
        }

        /// <summary>
        /// Creates two way Binding between two NDPropertys
        /// </summary>
        /// <typeparam name="TValueSource">The type of the source Property</typeparam>
        /// <typeparam name="TTypeSource">The type of the source object</typeparam>
        /// <typeparam name="TValueDestination">The type of the destination Property</typeparam>
        /// <typeparam name="TTypeDestination">The type of the destination object</typeparam>
        /// <param name="source">The source Object</param>
        /// <param name="sourceProperty">The source Property</param>
        /// <param name="destination">The destination Object</param>
        /// <param name="destinationProperty">The destination Property</param>
        /// <param name="converter">A converter that is used to translate from source property type to destination proeprty type.</param>
        /// <returns>The binding object</returns>
        public static TwoWayBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination> TwoWay<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination>(TTypeSource source, NDPropertyKey<TKey, TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, NDPropertyKey<TKey, TValueDestination, TTypeDestination> destinationProperty, ITwoWayConverter<TValueSource, TValueDestination> converter)
            where TTypeSource : class
            where TTypeDestination : class
        {
            return new TwoWayBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination>(source, sourceProperty, destination, destinationProperty, converter);
        }
        /// <summary>
        /// Creates two way Binding between two NDPropertys
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TTypeSource">The type of the source object</typeparam>
        /// <typeparam name="TTypeDestination">The type of the destination object</typeparam>
        /// <param name="source">The source Object</param>
        /// <param name="sourceProperty">The source Property</param>
        /// <param name="destination">The destination Object</param>
        /// <param name="destinationProperty">The destination Property</param>
        /// <returns>The binding object</returns>
        public static TwoWayBinding<TKey, TValue, TTypeSource, TValue, TTypeDestination> TwoWay<TKey, TValue, TTypeSource, TTypeDestination>(TTypeSource source, NDPropertyKey<TKey, TValue, TTypeSource> sourceProperty, TTypeDestination destination, NDPropertyKey<TKey, TValue, TTypeDestination> destinationProperty)
            where TTypeSource : class
            where TTypeDestination : class
        {
            return new TwoWayBinding<TKey, TValue, TTypeSource, TValue, TTypeDestination>(source, sourceProperty, destination, destinationProperty, new IdentetyConverter<TValue>());
        }

        /// <summary>
        /// Creates two way Binding between a NDProperty and a POCO
        /// </summary>
        /// <typeparam name="TValueSource">The type of the source Property</typeparam>
        /// <typeparam name="TTypeSource">The type of the source object</typeparam>
        /// <typeparam name="TValueDestination">The type of the destination Property</typeparam>
        /// <typeparam name="TTypeDestination">The type of the destination object</typeparam>
        /// <param name="source">The source Object</param>
        /// <param name="sourceProperty">The source Property</param>
        /// <param name="destination">The destination Object</param>
        /// <param name="destinationProperty">The name of the destination Property</param>
        /// <param name="converter">A converter that is used to translate from source property type to destination proeprty type.</param>
        /// <returns>The binding object</returns>
        public static TwoWayNotifyBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination> TwoWay<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination>(TTypeSource source, NDPropertyKey<TKey, TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, string destinationProperty, ITwoWayConverter<TValueSource, TValueDestination> converter)
            where TTypeSource : class
            where TTypeDestination : INotifyPropertyChanged
        {
            return new TwoWayNotifyBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination>(source, sourceProperty, destination, destinationProperty, converter);
        }
        /// <summary>
        /// Creates two way Binding between a NDProperty and a POCO
        /// </summary>
        /// <typeparam name="TValue">The type of the Property</typeparam>
        /// <typeparam name="TTypeSource">The type of the source object</typeparam>
        /// <typeparam name="TTypeDestination">The type of the destination object</typeparam>
        /// <param name="source">The source Object</param>
        /// <param name="sourceProperty">The source Property</param>
        /// <param name="destination">The destination Object</param>
        /// <param name="destinationProperty">The name of the destination Property</param>
        /// <returns>The binding object</returns>
        public static TwoWayNotifyBinding<TKey, TValue, TTypeSource, TValue, TTypeDestination> TwoWay<TKey, TValue, TTypeSource, TTypeDestination>(TTypeSource source, NDPropertyKey<TKey, TValue, TTypeSource> sourceProperty, TTypeDestination destination, string destinationProperty)
            where TTypeSource : class
            where TTypeDestination : INotifyPropertyChanged
        {
            return new TwoWayNotifyBinding<TKey, TValue, TTypeSource, TValue, TTypeDestination>(source, sourceProperty, destination, destinationProperty, new IdentetyConverter<TValue>());
        }

    }

    public class OneWayBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination> : IDisposable
        where TTypeSource : class
        where TTypeDestination : class
    {

        public NDReadOnlyPropertyKey<TKey, TValueSource, TTypeSource> SourceProperty { get; }
        public NDPropertyKey<TKey, TValueDestination, TTypeDestination> DestinationProperty { get; }
        public TTypeSource Source { get; }
        public TTypeDestination Destination { get; }
        public IConverter<TValueSource, TValueDestination> Converter { get; }
        /// <summary>Record Constructor</summary>
        /// <param name="source"><see cref="Source"/></param>
        /// <param name="sourceProperty"><see cref="SourceProperty"/></param>
        /// <param name="destination"><see cref="Destination"/></param>
        /// <param name="destinationProperty"><see cref="DestinationProperty"/></param>
        /// <param name="converter"><see cref="Converter"/></param>
        internal OneWayBinding(TTypeSource source, NDReadOnlyPropertyKey<TKey, TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, NDPropertyKey<TKey, TValueDestination, TTypeDestination> destinationProperty, IConverter<TValueSource, TValueDestination> converter)
        {
            SourceProperty = sourceProperty;
            DestinationProperty = destinationProperty;
            Source = source;
            Destination = destination;
            Converter = converter;
            PropertyRegistar<TKey>.AddEventHandler(SourceProperty, Source, SourceChanged);
        }
        private void SourceChanged(object sender, ChangedEventArgs<TValueSource, TTypeSource> e)
        {
            PropertyRegistar<TKey>.SetValue(DestinationProperty, Destination, Converter.ConvertTo(e.NewValue));
        }
        public void Dispose()
        {
            PropertyRegistar<TKey>.RemoveEventHandler(SourceProperty, Source, SourceChanged);
        }
    }

    public class TwoWayBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination> : IDisposable
        where TTypeSource : class
        where TTypeDestination : class
    {

        public NDPropertyKey<TKey, TValueSource, TTypeSource> SourceProperty { get; }
        public NDPropertyKey<TKey, TValueDestination, TTypeDestination> DestinationProperty { get; }
        public TTypeSource Source { get; }
        public TTypeDestination Destination { get; }
        public ITwoWayConverter<TValueSource, TValueDestination> Converter { get; }
        /// <summary>Record Constructor</summary>
        /// <param name="source"><see cref="Source"/></param>
        /// <param name="sourceProperty"><see cref="SourceProperty"/></param>
        /// <param name="destination"><see cref="Destination"/></param>
        /// <param name="destinationProperty"><see cref="DestinationProperty"/></param>
        /// <param name="converter"><see cref="Converter"/></param>
        internal TwoWayBinding(TTypeSource source, NDPropertyKey<TKey, TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, NDPropertyKey<TKey, TValueDestination, TTypeDestination> destinationProperty, ITwoWayConverter<TValueSource, TValueDestination> converter)
        {
            SourceProperty = sourceProperty;
            DestinationProperty = destinationProperty;
            Source = source;
            Destination = destination;
            Converter = converter;

            PropertyRegistar<TKey>.AddEventHandler(SourceProperty, Source, SourceChanged);
            PropertyRegistar<TKey>.AddEventHandler(DestinationProperty, Destination, DestinationChanged);

        }

        private void DestinationChanged(object sender, ChangedEventArgs<TValueDestination, TTypeDestination> e)
        {
            PropertyRegistar<TKey>.SetValue(SourceProperty, Source, Converter.ConvertTo(e.NewValue));
        }

        private void SourceChanged(object sender, ChangedEventArgs<TValueSource, TTypeSource> e)
        {
            PropertyRegistar<TKey>.SetValue(DestinationProperty, Destination, Converter.ConvertTo(e.NewValue));
        }

        public void Dispose()
        {
            PropertyRegistar<TKey>.RemoveEventHandler(SourceProperty, Source, SourceChanged);
            PropertyRegistar<TKey>.RemoveEventHandler(DestinationProperty, Destination, DestinationChanged);
        }
    }

    public class TwoWayNotifyBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination> : IDisposable
        where TTypeSource : class
        where TTypeDestination : INotifyPropertyChanged
    {

        public NDPropertyKey<TKey, TValueSource, TTypeSource> SourceProperty { get; }
        public string DestinationProperty { get; }
        public TTypeSource Source { get; }
        public TTypeDestination Destination { get; }
        public ITwoWayConverter<TValueSource, TValueDestination> Converter { get; }

        private Action<TTypeDestination, TValueDestination> setDestinationProperty;
        private Func<TTypeDestination, TValueDestination> getDestinationProperty;


        private Action<TTypeDestination, TValueDestination> GenerateSetter()
        {
            var param = Expression.Parameter(typeof(TTypeDestination), "obj");
            var param2 = Expression.Parameter(typeof(TValueDestination), "value");
            var assigne = Expression.Assign(Expression.Property(param, DestinationProperty), param2);
            var lambda = Expression.Lambda<Action<TTypeDestination, TValueDestination>>(assigne, param, param2);
            return lambda.Compile();
        }
        private Func<TTypeDestination, TValueDestination> GenerateGetter()
        {
            var param = Expression.Parameter(typeof(TTypeDestination), "obj");
            var lambda = Expression.Lambda<Func<TTypeDestination, TValueDestination>>(Expression.Property(param, DestinationProperty), param);
            return lambda.Compile();
        }
        /// <summary>Record Constructor</summary>
        /// <param name="source"><see cref="Source"/></param>
        /// <param name="sourceProperty"><see cref="SourceProperty"/></param>
        /// <param name="destination"><see cref="Destination"/></param>
        /// <param name="destinationProperty"><see cref="DestinationProperty"/></param>
        /// <param name="converter"><see cref="Converter"/></param>
        internal TwoWayNotifyBinding(TTypeSource source, NDPropertyKey<TKey, TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, string destinationProperty, ITwoWayConverter<TValueSource, TValueDestination> converter)
        {
            SourceProperty = sourceProperty;
            DestinationProperty = destinationProperty;
            Source = source;
            Destination = destination;
            Converter = converter;

            this.getDestinationProperty = GenerateGetter();
            this.setDestinationProperty = GenerateSetter();

            PropertyRegistar<TKey>.AddEventHandler(SourceProperty, Source, SourceChanged);
            Destination.PropertyChanged += Destination_PropertyChanged;
        }

        private void Destination_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == DestinationProperty)
                PropertyRegistar<TKey>.SetValue(SourceProperty, Source, Converter.ConvertTo(this.getDestinationProperty(Destination)));
        }

        private void SourceChanged(object sender, ChangedEventArgs<TValueSource, TTypeSource> e)
        {
            this.setDestinationProperty(Destination, Converter.ConvertTo(e.NewValue));
        }

        public void Dispose()
        {
            PropertyRegistar<TKey>.RemoveEventHandler(SourceProperty, Source, SourceChanged);
            Destination.PropertyChanged -= Destination_PropertyChanged;
        }
    }
    public class NotifyToNDPBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination> : IDisposable
        where TTypeDestination : class
        where TTypeSource : INotifyPropertyChanged
    {

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == SourceProperty)
                PropertyRegistar<TKey>.SetValue(DestinationProperty, Destination, Converter.ConvertTo(this.getSourceProperty(Source)));
        }

        public NDPropertyKey<TKey, TValueDestination, TTypeDestination> DestinationProperty { get; }
        public string SourceProperty { get; }
        public TTypeSource Source { get; }
        public TTypeDestination Destination { get; }
        public IConverter<TValueSource, TValueDestination> Converter { get; }

        private Func<TTypeSource, TValueSource> getSourceProperty;

        private Func<TTypeSource, TValueSource> GenerateGetter()
        {
            var param = Expression.Parameter(typeof(TTypeSource), "obj");
            var lambda = Expression.Lambda<Func<TTypeSource, TValueSource>>(Expression.Property(param, SourceProperty), param);
            return lambda.Compile();
        }

        public void Dispose()
        {
            Source.PropertyChanged -= Source_PropertyChanged;
        }
        /// <summary>Record Constructor</summary>
        /// <param name="destinationProperty"><see cref="DestinationProperty"/></param>
        /// <param name="sourceProperty"><see cref="SourceProperty"/></param>
        /// <param name="source"><see cref="Source"/></param>
        /// <param name="destination"><see cref="Destination"/></param>
        /// <param name="converter"><see cref="Converter"/></param>
        internal NotifyToNDPBinding(TTypeSource source, string sourceProperty, TTypeDestination destination, NDPropertyKey<TKey, TValueDestination, TTypeDestination> destinationProperty, IConverter<TValueSource, TValueDestination> converter)
        {
            DestinationProperty = destinationProperty;
            SourceProperty = sourceProperty;
            Source = source;
            Destination = destination;
            Converter = converter;
            this.getSourceProperty = GenerateGetter();
            source.PropertyChanged += Source_PropertyChanged;
        }
    }

    public class NDPToNotifyBinding<TKey, TValueSource, TTypeSource, TValueDestination, TTypeDestination> : IDisposable
        where TTypeSource : class
        where TTypeDestination : INotifyPropertyChanged
    {

        public NDReadOnlyPropertyKey<TKey, TValueSource, TTypeSource> SourceProperty { get; }
        public string DestinationProperty { get; }
        public TTypeSource Source { get; }
        public TTypeDestination Destination { get; }
        public IConverter<TValueSource, TValueDestination> Converter { get; }

        private Action<TTypeDestination, TValueDestination> setDestinationProperty;


        private Action<TTypeDestination, TValueDestination> GenerateSetter()
        {
            var param = Expression.Parameter(typeof(TTypeDestination), "obj");
            var param2 = Expression.Parameter(typeof(TValueDestination), "value");
            var assigne = Expression.Assign(Expression.Property(param, DestinationProperty), param2);
            var lambda = Expression.Lambda<Action<TTypeDestination, TValueDestination>>(assigne, param, param2);
            return lambda.Compile();
        }

        public void Dispose()
        {
            PropertyRegistar<TKey>.RemoveEventHandler(SourceProperty, Source, PropertyChanged);
        }

        /// <summary>Record Constructor</summary>
        /// <param name="source"><see cref="Source"/></param>
        /// <param name="sourceProperty"><see cref="SourceProperty"/></param>
        /// <param name="destination"><see cref="Destination"/></param>
        /// <param name="destinationProperty"><see cref="DestinationProperty"/></param>
        /// <param name="converter"><see cref="Converter"/></param>
        internal NDPToNotifyBinding(TTypeSource source, NDReadOnlyPropertyKey<TKey, TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, string destinationProperty, IConverter<TValueSource, TValueDestination> converter)
        {
            SourceProperty = sourceProperty;
            DestinationProperty = destinationProperty;
            Source = source;
            Destination = destination;
            Converter = converter;

            this.setDestinationProperty = GenerateSetter();

            PropertyRegistar<TKey>.AddEventHandler(SourceProperty, Source, PropertyChanged);


        }

        private void PropertyChanged(object sender, ChangedEventArgs<TValueSource, TTypeSource> e)
        {
            this.setDestinationProperty(Destination, Converter.ConvertTo(e.NewValue));
        }
    }


}