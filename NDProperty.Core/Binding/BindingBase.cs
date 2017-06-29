using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace NDProperty.Binding
{

    public static class Bind
    {

        public static OneWayBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination> OneWay<TValueSource, TTypeSource, TValueDestination, TTypeDestination>(TTypeSource source, NDReadOnlyPropertyKey<TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, NDPropertyKey<TValueDestination, TTypeDestination> destinationProperty, IConverter<TValueSource, TValueDestination> converter)
            where TTypeSource : class
            where TTypeDestination : class
        {
            return new OneWayBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination>(source, sourceProperty, destination, destinationProperty, converter);
        }
        public static OneWayBinding<TValue, TTypeSource, TValue, TTypeDestination> OneWay<TValue, TTypeSource, TTypeDestination>(TTypeSource source, NDReadOnlyPropertyKey<TValue, TTypeSource> sourceProperty, TTypeDestination destination, NDPropertyKey<TValue, TTypeDestination> destinationProperty)
            where TTypeSource : class
            where TTypeDestination : class
        {
            return new OneWayBinding<TValue, TTypeSource, TValue, TTypeDestination>(source, sourceProperty, destination, destinationProperty, new IdentetyConverter<TValue>());
        }

        public static NotifyToNDPBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination> OneWay<TValueSource, TTypeSource, TValueDestination, TTypeDestination>(TTypeSource source, string sourceProperty, TTypeDestination destination, NDPropertyKey<TValueDestination, TTypeDestination> destinationProperty, IConverter<TValueSource, TValueDestination> converter)
            where TTypeSource : INotifyPropertyChanged
            where TTypeDestination : class
        {
            return new NotifyToNDPBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination>(source, sourceProperty, destination, destinationProperty, converter);
        }
        public static NotifyToNDPBinding<TValue, TTypeSource, TValue, TTypeDestination> OneWay<TValue, TTypeSource, TTypeDestination>(TTypeSource source, string sourceProperty, TTypeDestination destination, NDPropertyKey<TValue, TTypeDestination> destinationProperty)
            where TTypeSource : INotifyPropertyChanged
            where TTypeDestination : class
        {
            return new NotifyToNDPBinding<TValue, TTypeSource, TValue, TTypeDestination>(source, sourceProperty, destination, destinationProperty, new IdentetyConverter<TValue>());
        }

        public static NDPToNotifyBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination> OneWay<TValueSource, TTypeSource, TValueDestination, TTypeDestination>(TTypeSource source, NDReadOnlyPropertyKey<TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, string destinationProperty, IConverter<TValueSource, TValueDestination> converter)
            where TTypeSource : class
            where TTypeDestination : INotifyPropertyChanged
        {
            return new NDPToNotifyBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination>(source, sourceProperty, destination, destinationProperty, converter);
        }
        public static NDPToNotifyBinding<TValue, TTypeSource, TValue, TTypeDestination> OneWay<TValue, TTypeSource, TTypeDestination>(TTypeSource source, NDReadOnlyPropertyKey<TValue, TTypeSource> sourceProperty, TTypeDestination destination, string destinationProperty)
            where TTypeSource : class
            where TTypeDestination : INotifyPropertyChanged
        {
            return new NDPToNotifyBinding<TValue, TTypeSource, TValue, TTypeDestination>(source, sourceProperty, destination, destinationProperty, new IdentetyConverter<TValue>());
        }

        public static TwoWayBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination> TwoWay<TValueSource, TTypeSource, TValueDestination, TTypeDestination>(TTypeSource source, NDPropertyKey<TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, NDPropertyKey<TValueDestination, TTypeDestination> destinationProperty, ITwoWayConverter<TValueSource, TValueDestination> converter)
            where TTypeSource : class
            where TTypeDestination : class
        {
            return new TwoWayBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination>(source, sourceProperty, destination, destinationProperty, converter);
        }
        public static TwoWayBinding<TValue, TTypeSource, TValue, TTypeDestination> TwoWay<TValue, TTypeSource, TTypeDestination>(TTypeSource source, NDPropertyKey<TValue, TTypeSource> sourceProperty, TTypeDestination destination, NDPropertyKey<TValue, TTypeDestination> destinationProperty)
            where TTypeSource : class
            where TTypeDestination : class
        {
            return new TwoWayBinding<TValue, TTypeSource, TValue, TTypeDestination>(source, sourceProperty, destination, destinationProperty, new IdentetyConverter<TValue>());
        }

        public static TwoWayNotifyBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination> TwoWay<TValueSource, TTypeSource, TValueDestination, TTypeDestination>(TTypeSource source, NDPropertyKey<TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, string destinationProperty, ITwoWayConverter<TValueSource, TValueDestination> converter)
            where TTypeSource : class
            where TTypeDestination : INotifyPropertyChanged
        {
            return new TwoWayNotifyBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination>(source, sourceProperty, destination, destinationProperty, converter);
        }
        public static TwoWayNotifyBinding<TValue, TTypeSource, TValue, TTypeDestination> TwoWay<TValue, TTypeSource, TTypeDestination>(TTypeSource source, NDPropertyKey<TValue, TTypeSource> sourceProperty, TTypeDestination destination, string destinationProperty)
            where TTypeSource : class
            where TTypeDestination : INotifyPropertyChanged
        {
            return new TwoWayNotifyBinding<TValue, TTypeSource, TValue, TTypeDestination>(source, sourceProperty, destination, destinationProperty, new IdentetyConverter<TValue>());
        }

    }

    public class OneWayBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination> : IDisposable
        where TTypeSource : class
        where TTypeDestination : class
    {

        public NDReadOnlyPropertyKey<TValueSource, TTypeSource> SourceProperty { get; }
        public NDPropertyKey<TValueDestination, TTypeDestination> DestinationProperty { get; }
        public TTypeSource Source { get; }
        public TTypeDestination Destination { get; }
        public IConverter<TValueSource, TValueDestination> Converter { get; }
        /// <summary>Record Constructor</summary>
        /// <param name="source"><see cref="Source"/></param>
        /// <param name="sourceProperty"><see cref="SourceProperty"/></param>
        /// <param name="destination"><see cref="Destination"/></param>
        /// <param name="destinationProperty"><see cref="DestinationProperty"/></param>
        /// <param name="converter"><see cref="Converter"/></param>
        internal OneWayBinding(TTypeSource source, NDReadOnlyPropertyKey<TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, NDPropertyKey<TValueDestination, TTypeDestination> destinationProperty, IConverter<TValueSource, TValueDestination> converter)
        {
            SourceProperty = sourceProperty;
            DestinationProperty = destinationProperty;
            Source = source;
            Destination = destination;
            Converter = converter;
            PropertyRegistar.AddEventHandler(SourceProperty, Source, SourceChanged);
        }
        private void SourceChanged(object sender, ChangedEventArgs<TValueSource, TTypeSource> e)
        {
            PropertyRegistar.SetValue(DestinationProperty, Destination, Converter.ConvertTo(e.NewValue));
        }
        public void Dispose()
        {
            PropertyRegistar.RemoveEventHandler(SourceProperty, Source, SourceChanged);
        }
    }

    public class TwoWayBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination> : IDisposable
        where TTypeSource : class
        where TTypeDestination : class
    {

        public NDPropertyKey<TValueSource, TTypeSource> SourceProperty { get; }
        public NDPropertyKey<TValueDestination, TTypeDestination> DestinationProperty { get; }
        public TTypeSource Source { get; }
        public TTypeDestination Destination { get; }
        public ITwoWayConverter<TValueSource, TValueDestination> Converter { get; }
        /// <summary>Record Constructor</summary>
        /// <param name="source"><see cref="Source"/></param>
        /// <param name="sourceProperty"><see cref="SourceProperty"/></param>
        /// <param name="destination"><see cref="Destination"/></param>
        /// <param name="destinationProperty"><see cref="DestinationProperty"/></param>
        /// <param name="converter"><see cref="Converter"/></param>
        internal TwoWayBinding(TTypeSource source, NDPropertyKey<TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, NDPropertyKey<TValueDestination, TTypeDestination> destinationProperty, ITwoWayConverter<TValueSource, TValueDestination> converter)
        {
            SourceProperty = sourceProperty;
            DestinationProperty = destinationProperty;
            Source = source;
            Destination = destination;
            Converter = converter;

            PropertyRegistar.AddEventHandler(SourceProperty, Source, SourceChanged);
            PropertyRegistar.AddEventHandler(DestinationProperty, Destination, DestinationChanged);

        }

        private void DestinationChanged(object sender, ChangedEventArgs<TValueDestination, TTypeDestination> e)
        {
            PropertyRegistar.SetValue(SourceProperty, Source, Converter.ConvertTo(e.NewValue));
        }

        private void SourceChanged(object sender, ChangedEventArgs<TValueSource, TTypeSource> e)
        {
            PropertyRegistar.SetValue(DestinationProperty, Destination, Converter.ConvertTo(e.NewValue));
        }

        public void Dispose()
        {
            PropertyRegistar.RemoveEventHandler(SourceProperty, Source, SourceChanged);
            PropertyRegistar.RemoveEventHandler(DestinationProperty, Destination, DestinationChanged);
        }
    }

    public class TwoWayNotifyBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination> : IDisposable
        where TTypeSource : class
        where TTypeDestination : INotifyPropertyChanged
    {

        public NDPropertyKey<TValueSource, TTypeSource> SourceProperty { get; }
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
        internal TwoWayNotifyBinding(TTypeSource source, NDPropertyKey<TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, string destinationProperty, ITwoWayConverter<TValueSource, TValueDestination> converter)
        {
            SourceProperty = sourceProperty;
            DestinationProperty = destinationProperty;
            Source = source;
            Destination = destination;
            Converter = converter;

            this.getDestinationProperty = GenerateGetter();
            this.setDestinationProperty = GenerateSetter();

            PropertyRegistar.AddEventHandler(SourceProperty, Source, SourceChanged);
            Destination.PropertyChanged += Destination_PropertyChanged;
        }

        private void Destination_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == DestinationProperty)
                PropertyRegistar.SetValue(SourceProperty, Source, Converter.ConvertTo(this.getDestinationProperty(Destination)));
        }

        private void SourceChanged(object sender, ChangedEventArgs<TValueSource, TTypeSource> e)
        {
            this.setDestinationProperty(Destination, Converter.ConvertTo(e.NewValue));
        }

        public void Dispose()
        {
            PropertyRegistar.RemoveEventHandler(SourceProperty, Source, SourceChanged);
            Destination.PropertyChanged -= Destination_PropertyChanged;
        }
    }
    public class NotifyToNDPBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination> : IDisposable
        where TTypeDestination : class
        where TTypeSource : INotifyPropertyChanged
    {

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == SourceProperty)
                PropertyRegistar.SetValue(DestinationProperty, Destination, Converter.ConvertTo(this.getSourceProperty(Source)));
        }

        public NDPropertyKey<TValueDestination, TTypeDestination> DestinationProperty { get; }
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
        internal NotifyToNDPBinding(TTypeSource source, string sourceProperty, TTypeDestination destination, NDPropertyKey<TValueDestination, TTypeDestination> destinationProperty, IConverter<TValueSource, TValueDestination> converter)
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

    public class NDPToNotifyBinding<TValueSource, TTypeSource, TValueDestination, TTypeDestination> : IDisposable
        where TTypeSource : class
        where TTypeDestination : INotifyPropertyChanged
    {

        public NDReadOnlyPropertyKey<TValueSource, TTypeSource> SourceProperty { get; }
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
            PropertyRegistar.RemoveEventHandler(SourceProperty, Source, PropertyChanged);
        }

        /// <summary>Record Constructor</summary>
        /// <param name="source"><see cref="Source"/></param>
        /// <param name="sourceProperty"><see cref="SourceProperty"/></param>
        /// <param name="destination"><see cref="Destination"/></param>
        /// <param name="destinationProperty"><see cref="DestinationProperty"/></param>
        /// <param name="converter"><see cref="Converter"/></param>
        internal NDPToNotifyBinding(TTypeSource source, NDReadOnlyPropertyKey<TValueSource, TTypeSource> sourceProperty, TTypeDestination destination, string destinationProperty, IConverter<TValueSource, TValueDestination> converter)
        {
            SourceProperty = sourceProperty;
            DestinationProperty = destinationProperty;
            Source = source;
            Destination = destination;
            Converter = converter;

            this.setDestinationProperty = GenerateSetter();

            PropertyRegistar.AddEventHandler(SourceProperty, Source, PropertyChanged);


        }

        private void PropertyChanged(object sender, ChangedEventArgs<TValueSource, TTypeSource> e)
        {
            this.setDestinationProperty(Destination, Converter.ConvertTo(e.NewValue));
        }
    }


}