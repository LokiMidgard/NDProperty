using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NDProperty.Propertys;
using NDProperty.Providers;
using NDProperty.Utils;

namespace NDProperty.Providers.Binding
{


    public static partial class Binding
    {

        //    public static Binding<TKey, TSourceType, TSourceValue, TTargetType, TTargetValue> Bind<TKey, TSourceType, TSourceValue, TTargetType, TTargetValue>(this TSourceType source, NDProperty.Propertys.NDReadOnlyPropertyKey<TKey, TSourceType, TSourceValue> property)
        //where TTargetType : class
        //where TSourceType : class
        //    {
        //        throw new NotImplementedException();

        //    }

        public static Binding<TKey, TSourceType, TSourceValue, TTargetType, TTargetValue, TPropertyType> Bind<TKey, TSourceType, TSourceValue, TTargetType, TTargetValue, TPropertyType>(this TPropertyType property, TSourceType source, IBindingConfiguration<TKey, TSourceValue, TTargetType, TTargetValue> configuration)
            where TPropertyType : NDBasePropertyKey<TKey, TSourceType, TSourceValue>, INDProperty<TKey, TSourceType, TSourceValue>
            where TTargetType : class
            where TSourceType : class
        {
            return new Binding<TKey, TSourceType, TSourceValue, TTargetType, TTargetValue, TPropertyType>(source, property, configuration as IInternalBindingConfiguration<TKey, TSourceValue, TTargetType, TTargetValue>);

            throw new NotImplementedException();

        }


        public static IBindingConfigurator<TKey, TType, TValue> Of<TKey, TType, TValue>(this NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType of)
            where TType : class
        {
            return new Start<TKey, TType, TValue>(property, of);
        }
        public static IBindingConfiguratorWritable<TKey, TType, TValue> Of<TKey, TType, TValue>(this NDBasePropertyKey<TKey, TType, TValue> property, TType of)
            where TType : class
        {
            return new StartWritable<TKey, TType, TValue>(property, of);
        }
        internal interface IInternalBindingConfiguration<TKey, TSourceValue, TTargetType, TTargetValue> : IBindingConfiguration<TKey, TSourceValue, TTargetType, TTargetValue>
                where TTargetType : class

        {
            event Action<TSourceValue> NewValue;
            void Generate<TSourceType>(TSourceType source, NDBasePropertyKey<TKey, TSourceType, TSourceValue> bindingProperty) where TSourceType : class;
            void Destroy();
        }

        private class Start<TKey, TType, TValue> : BindingConfiguration<TKey, TType, TValue>
            where TType : class
        {
            public readonly TType of;

            public Start(NDReadOnlyPropertyKey<TKey, TType, TValue> property, TType of) : base(property)
            {
                this.of = of;
            }

            internal override void Generate<TSourceType, TSourceValue>(TSourceType source, NDBasePropertyKey<TKey, TSourceType, TSourceValue> bindingProperty) => base.Register(this.of);

            internal override void UnRegister() { }

            internal override void Register(object obj) { }
        }
        private class StartWritable<TKey, TType, TValue> : BindingConfiguration<TKey, TType, TValue>, IBindingConfiguratorWritable<TKey, TType, TValue>, IHaveWritableProperty<TKey, TType, TValue>
            where TType : class
        {
            public readonly TType of;

            public new NDBasePropertyKey<TKey, TType, TValue> Property { get; }

            public StartWritable(NDBasePropertyKey<TKey, TType, TValue> property, TType of) : base(property)
            {
                this.of = of;
                Property = property;
            }

            internal override void Generate<TSourceType, TSourceValue>(TSourceType source, NDBasePropertyKey<TKey, TSourceType, TSourceValue> bindingProperty) => base.Register(this.of);

            internal override void UnRegister() { }

            internal override void Register(object obj) { }

            public IBindingConfiguration<TKey, TValue, TType, TValue> TwoWay()
            {
                return new EndWritable<TKey, TType, TValue>(this);
            }

            public IBindingConfiguration<TKey, TSourceValue, TType, TValue> ConvertTwoWay<TSourceValue>(Func<TValue, TSourceValue> converter, Func<TSourceValue, TValue> converterback)
            {
                return new EndWritable<TKey, TType, TSourceValue, TValue>(this, converter, converterback);

            }
        }

        private class Over<TKey, TType, TValue, TOldType, TOldValue> : BindingConfiguration<TKey, TType, TValue>
            where TType : class, TOldValue
            where TOldType : class
        {
            private readonly BindingConfiguration<TKey, TOldType, TOldValue> oldBindingConfiguration;

            public Over(NDReadOnlyPropertyKey<TKey, TType, TValue> property, BindingConfiguration<TKey, TOldType, TOldValue> oldBindingConfiguration) : base(property)
            {
                this.oldBindingConfiguration = oldBindingConfiguration;
            }

            internal override void Generate<TSourceType, TSourceValue>(TSourceType source, NDBasePropertyKey<TKey, TSourceType, TSourceValue> bindingProperty) => this.oldBindingConfiguration.Generate(source, bindingProperty);

            internal override void Destroy()
            {
                base.Destroy();
                this.oldBindingConfiguration.Destroy();
            }

        }
        private interface IHaveWritableProperty<TKey, TType, TValue> where TType : class
        {
            NDBasePropertyKey<TKey, TType, TValue> Property { get; }
        }
        private class OverWritable<TKey, TType, TValue, TOldType, TOldValue> : Over<TKey, TType, TValue, TOldType, TOldValue>, IBindingConfiguratorWritable<TKey, TType, TValue>, IHaveWritableProperty<TKey, TType, TValue> where TType : class, TOldValue
    where TOldType : class
        {
            public new NDBasePropertyKey<TKey, TType, TValue> Property { get; }

            public OverWritable(NDBasePropertyKey<TKey, TType, TValue> property, BindingConfiguration<TKey, TOldType, TOldValue> oldBindingConfiguration) : base(property, oldBindingConfiguration)
            {
                Property = property;
            }

            public IBindingConfiguration<TKey, TValue, TType, TValue> TwoWay()
            {
                return new EndWritable<TKey, TType, TValue>(this);
            }

            public IBindingConfiguration<TKey, TSourceValue, TType, TValue> ConvertTwoWay<TSourceValue>(Func<TValue, TSourceValue> converter, Func<TSourceValue, TValue> converterback)
            {
                return new EndWritable<TKey, TType, TSourceValue, TValue>(this, converter, converterback);
            }
        }




        private class End<TKey, TSourceValue, TTargetType, TTargetValue> : IInternalBindingConfiguration<TKey, TSourceValue, TTargetType, TTargetValue>
            where TTargetType : class
        {
            public readonly BindingConfiguration<TKey, TTargetType, TTargetValue> bindingConfiguration;
            public readonly Func<TTargetValue, TSourceValue> converter;

            public event Action<TSourceValue> NewValue;

            public End(BindingConfiguration<TKey, TTargetType, TTargetValue> bindingConfiguration, Func<TTargetValue, TSourceValue> converter)
            {
                this.bindingConfiguration = bindingConfiguration ?? throw new ArgumentNullException(nameof(bindingConfiguration));
                this.converter = converter ?? throw new ArgumentNullException(nameof(converter));

                bindingConfiguration.NewValueRecived += BindingConfiguration_NewValueRecived;
            }

            private void BindingConfiguration_NewValueRecived(TTargetValue obj)
            {
                var convertedValue = this.converter(obj);
                NewValue?.Invoke(convertedValue);
            }

            public void Generate<TSourceType>(TSourceType source, NDBasePropertyKey<TKey, TSourceType, TSourceValue> bindingProperty) where TSourceType : class => this.bindingConfiguration.Generate(source, bindingProperty);
            public void Destroy() => this.bindingConfiguration.Destroy();
        }

        private class End<TKey, TSourceValue, TTargetType> : IInternalBindingConfiguration<TKey, TSourceValue, TTargetType, TSourceValue>
    where TTargetType : class
        {
            public readonly BindingConfiguration<TKey, TTargetType, TSourceValue> bindingConfiguration;

            public event Action<TSourceValue> NewValue;

            public End(BindingConfiguration<TKey, TTargetType, TSourceValue> bindingConfiguration)
            {
                this.bindingConfiguration = bindingConfiguration ?? throw new ArgumentNullException(nameof(bindingConfiguration));

                bindingConfiguration.NewValueRecived += BindingConfiguration_NewValueRecived;
            }

            private void BindingConfiguration_NewValueRecived(TSourceValue obj)
            {
                NewValue?.Invoke(obj);
            }

            public void Generate<TSourceType>(TSourceType source, NDBasePropertyKey<TKey, TSourceType, TSourceValue> bindingProperty) where TSourceType : class => this.bindingConfiguration.Generate(source, bindingProperty);
            public void Destroy() => this.bindingConfiguration.Destroy();

        }

        private class EndWritable<TKey, TType, TValue> : IInternalBindingConfiguration<TKey, TValue, TType, TValue>
            where TType : class

        {
            private readonly IHaveWritableProperty<TKey, TType, TValue> writableBindingConfiguration;
            private readonly BindingConfiguration<TKey, TType, TValue> bindingConfiguration;

            private Action destoryHandler;

            public event Action<TValue> NewValue;

            public EndWritable(BindingConfiguration<TKey, TType, TValue> bindingConfiguration)
            {
                this.bindingConfiguration = bindingConfiguration ?? throw new ArgumentNullException(nameof(bindingConfiguration));
                this.writableBindingConfiguration = bindingConfiguration as IHaveWritableProperty<TKey, TType, TValue>;
                bindingConfiguration.NewValueRecived += BindingConfiguration_NewValueRecived;
            }

            private void BindingConfiguration_NewValueRecived(TValue obj)
            {
                NewValue?.Invoke(obj);
            }

            public void Generate<TSourceType>(TSourceType source, NDBasePropertyKey<TKey, TSourceType, TValue> bindingProperty)
                where TSourceType : class
            {
                this.bindingConfiguration.Generate(source, bindingProperty);
                LocalValueProvider<TKey>.Instance.AddEventHandler(bindingProperty, source, BindedObjectIsChanging);
                this.destoryHandler = () => LocalValueProvider<TKey>.Instance.RemoveEventHandler(bindingProperty, source, BindedObjectIsChanging);
            }

            private void BindedObjectIsChanging<TSourceType>(object sender, ChangedEventArgs<TKey, TSourceType, TValue> e)
                where TSourceType : class
            {
                LocalValueProvider<TKey>.Instance.SetValue(this.writableBindingConfiguration.Property, this.bindingConfiguration.CurrentObject, e.NewValue);
            }

            public void Destroy()
            {
                this.destoryHandler();
                this.bindingConfiguration.Destroy();
            }
        }


        private class EndWritable<TKey, TType, TSourceValue, TTargetValue> : IInternalBindingConfiguration<TKey, TSourceValue, TType, TTargetValue>
            where TType : class

        {
            private readonly IHaveWritableProperty<TKey, TType, TTargetValue> writableBindingConfiguration;
            private readonly BindingConfiguration<TKey, TType, TTargetValue> bindingConfiguration;

            private Func<TSourceValue, TTargetValue> convertBack;
            private Func<TTargetValue, TSourceValue> convert;

            private Action destoryHandler;

            public event Action<TSourceValue> NewValue;


            public EndWritable(BindingConfiguration<TKey, TType, TTargetValue> bindingConfiguration, Func<TTargetValue, TSourceValue> convert, Func<TSourceValue, TTargetValue> convertBack)
            {
                this.bindingConfiguration = bindingConfiguration ?? throw new ArgumentNullException(nameof(bindingConfiguration));
                this.convertBack = convertBack ?? throw new ArgumentNullException(nameof(convertBack));
                this.convert = convert ?? throw new ArgumentNullException(nameof(convert));
                this.writableBindingConfiguration = bindingConfiguration as IHaveWritableProperty<TKey, TType, TTargetValue>;

                bindingConfiguration.NewValueRecived += BindingConfiguration_NewValueRecived;
            }

            private void BindingConfiguration_NewValueRecived(TTargetValue obj)
            {
                var to = this.convert(obj);
                NewValue?.Invoke(to);
            }

            public void Generate<TSourceType>(TSourceType source, NDBasePropertyKey<TKey, TSourceType, TSourceValue> bindingProperty)
                where TSourceType : class
            {
                this.bindingConfiguration.Generate(source, bindingProperty);
                LocalValueProvider<TKey>.Instance.AddEventHandler(bindingProperty, source, BindedObjectIsChanging);
                this.destoryHandler = () => LocalValueProvider<TKey>.Instance.RemoveEventHandler(bindingProperty, source, BindedObjectIsChanging);
            }

            private void BindedObjectIsChanging<TSourceType>(object sender, ChangedEventArgs<TKey, TSourceType, TSourceValue> e)
                where TSourceType : class
            {
                var to = this.convertBack(e.NewValue);
                LocalValueProvider<TKey>.Instance.SetValue(this.writableBindingConfiguration.Property, this.bindingConfiguration.CurrentObject, to);
            }

            public void Destroy()
            {
                this.destoryHandler();
                this.bindingConfiguration.Destroy();
            }
        }


        private abstract class BindingConfiguration<TKey>
        {
            internal BindingConfiguration() { }
            internal abstract void Generate<TSourceType, TSourceValue>(TSourceType source, NDBasePropertyKey<TKey, TSourceType, TSourceValue> bindingProperty) where TSourceType : class;
            internal abstract void Register(object obj);
            internal abstract void UnRegister();

            internal abstract void Destroy();

        }
        private abstract class BindingConfiguration<TKey, TType, TValue> : BindingConfiguration<TKey>, IBindingConfigurator<TKey, TType, TValue> where TType : class
        {


            internal event Action<TValue> NewValueRecived;

            private BindingConfiguration<TKey> next;
            protected BindingConfiguration<TKey> Next => this.next;

            public TType CurrentObject { get; private set; }

            public NDReadOnlyPropertyKey<TKey, TType, TValue> Property { get; }

            internal BindingConfiguration() { }

            protected BindingConfiguration(NDReadOnlyPropertyKey<TKey, TType, TValue> property)
            {
                Property = property;
            }

            internal override void UnRegister()
            {
                if (this.CurrentObject != null)
                    NDProperty.PropertyRegistar<TKey>.RemoveEventHandler(this.Property, this.CurrentObject, Handler);
                this.CurrentObject = null;
            }

            internal override void Register(object obj)
            {
                if (!(obj is TType o))
                    throw new ArgumentException(nameof(obj));
                this.CurrentObject = o;

                NDProperty.PropertyRegistar<TKey>.AddEventHandler(this.Property, o, Handler);

                var newValue = NDProperty.PropertyRegistar<TKey>.GetValue(this.Property, o);

                RegisterNext(newValue);

            }

            private void Handler(object sender, ChangedEventArgs<TKey, TType, TValue> e)
            {
                var newValue = e.NewValue;
                RegisterNext(newValue);
            }

            private void RegisterNext(TValue newValue)
            {
                Next?.UnRegister();
                if (newValue != null)
                    Next?.Register(newValue);
                NewValueRecived?.Invoke(newValue);
            }

            internal override void Destroy() => UnRegister();


            public IBindingConfigurator<TKey, TNewType, TNewValue> Over<TNewType, TNewValue>(NDProperty.Propertys.NDReadOnlyPropertyKey<TKey, TNewType, TNewValue> property)
                where TNewType : class, TValue
            {
                var over = new Over<TKey, TNewType, TNewValue, TType, TValue>(property, this);
                this.next = over;
                return over;
            }

            public IBindingConfiguratorWritable<TKey, TNewType, TNewValue> Over<TNewType, TNewValue>(NDProperty.Propertys.NDBasePropertyKey<TKey, TNewType, TNewValue> property)
                         where TNewType : class, TValue
            {
                var over = new OverWritable<TKey, TNewType, TNewValue, TType, TValue>(property, this);
                this.next = over;
                return over;
            }


            public IBindingConfiguration<TKey, TValue, TType, TValue> OneWay()
            {
                return new End<TKey, TValue, TType>(this);

            }

            public IBindingConfiguration<TKey, TSourceValue, TType, TValue> ConvertOneWay<TSourceValue>(Func<TValue, TSourceValue> converter)
            {
                return new End<TKey, TSourceValue, TType, TValue>(this, converter);
            }

        }

    }

    public abstract class Binding<TKey, TSourceType, TSourceValue>
            where TSourceType : class
    {
        public NDReadOnlyPropertyKey<TKey, TSourceType, TSourceValue> Property { get; }

        internal Binding(TSourceType target, NDReadOnlyPropertyKey<TKey, TSourceType, TSourceValue> property)
        {
            Target = target;
            Property = property;
        }


        public TSourceType Target { get; }

        public TSourceValue CurrentValue { get; protected set; }

        public bool HasValue { get; protected set; }

    }
    public abstract class Binding<TKey, TSourceType, TSourceValue, TPropertyType> : Binding<TKey, TSourceType, TSourceValue>
           where TPropertyType : NDReadOnlyPropertyKey<TKey, TSourceType, TSourceValue>, INDProperty<TKey, TSourceType, TSourceValue>
           where TSourceType : class
    {
        public new TPropertyType Property { get; }

        internal Binding(TSourceType target, TPropertyType property) : base(target, property)
        {
            Property = property;
        }

    }
    public sealed class Binding<TKey, TSourceType, TSourceValue, TTargetType, TTargetValue, TPropertyType> : Binding<TKey, TSourceType, TSourceValue, TPropertyType>, IDisposable
            where TPropertyType : NDBasePropertyKey<TKey, TSourceType, TSourceValue>, INDProperty<TKey, TSourceType, TSourceValue>
            where TTargetType : class
            where TSourceType : class
    {


        private Binding.IInternalBindingConfiguration<TKey, TSourceValue, TTargetType, TTargetValue> configuration;

        internal Binding(TSourceType target, TPropertyType property, Binding.IInternalBindingConfiguration<TKey, TSourceValue, TTargetType, TTargetValue> configuration) : base(target, property)
        {
            this.configuration = configuration;
            configuration.Generate(target, property);
            configuration.NewValue += Configuration_NewValue;
            BindingProvider<TKey>.Instance.RegisterBinding(this);

        }

        private void Configuration_NewValue(TSourceValue obj)
        {

            BindingProvider<TKey>.Instance.Update(this, obj, true, () =>
            {
                CurrentValue = obj;
                HasValue = true;
                return true;
            });
        }

        internal void RemoveValue()
        {
            BindingProvider<TKey>.Instance.Update(this, default, false, () =>
            {
                HasValue = false;
                CurrentValue = default; return true;
            });
        }

        #region IDisposable Support
        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.


        public void Dispose()
        {
            if (!this.disposedValue)
            {
                this.configuration.Destroy();
                this.configuration.NewValue -= Configuration_NewValue;
                BindingProvider<TKey>.Instance.RemoveBinding(this);
                this.disposedValue = true;
            }
        }
        #endregion
    }

    //public class BindingConverter<TFrom, TTo>
    //{
    //    private Func<TFrom, TTo> convert;

    //    public BindingConverter(Func<TFrom, TTo> convert)
    //    {
    //        this.convert = convert ?? throw new ArgumentNullException(nameof(convert));
    //    }

    //    public TTo Convert(TFrom from) => this.convert(from);
    //}
    //public class BindingConverterTwoWay<TFrom, TTo> : BindingConverter<TFrom, TTo>
    //{
    //    private Func<TTo, TFrom> convertBack;

    //    public BindingConverterTwoWay(Func<TFrom, TTo> convert, Func<TTo, TFrom> convertBack) : base(convert)
    //    {
    //        this.convertBack = convertBack ?? throw new ArgumentNullException(nameof(convertBack));
    //    }

    //    public TFrom ConvertBack(TTo from) => this.convertBack(from);
    //}


    public class BindingProvider<TKey> : NDProperty.Providers.ValueProvider<TKey>
    {
        public static BindingProvider<TKey> Instance { get; } = new BindingProvider<TKey>();

        private BindingProvider() : base(false) { }


        private readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, Dictionary<object, object>> table = new ConditionalWeakTable<object, Dictionary<object, object>>();

        public override (TValue value, bool hasValue) GetValue<TType, TValue>(TType targetObject, NDProperty.Propertys.NDReadOnlyPropertyKey<TKey, TType, TValue> property)
        {
            Binding<TKey, TType, TValue> binding;

            binding = GetBinding(targetObject, property);

            if (binding != null)
                return (binding.CurrentValue, binding.HasValue);
            return (default, false);
        }

        private Binding<TKey, TType, TValue> GetBinding<TType, TValue>(TType targetObject, NDReadOnlyPropertyKey<TKey, TType, TValue> property) where TType : class
        {
            Binding<TKey, TType, TValue> binding;
            if (this.table.TryGetValue(targetObject, out var dictionary) && dictionary.ContainsKey(property))
            {
                binding = dictionary[property] as Binding<TKey, TType, TValue>;
                System.Diagnostics.Debug.Assert(object.ReferenceEquals(binding.Target, targetObject), "Binding and searched object arn't the same :/");
            }
            else
                binding = null;
            return binding;
        }

        internal bool Update<TType, TValue, TPropertyType>(Binding<TKey, TType, TValue, TPropertyType> binding, TValue newValue, bool hasNewValue, Func<bool> updateCode)
            where TType : class
            where TPropertyType : NDReadOnlyPropertyKey<TKey, TType, TValue>, INDProperty<TKey, TType, TValue>

        {
            return base.Update(binding, binding.Target, binding.Property, newValue, hasNewValue, updateCode);
        }

        internal void RegisterBinding<TSourceType, TSourceValue, TTargetType, TTargetValue, TPropertyType>(Binding<TKey, TSourceType, TSourceValue, TTargetType, TTargetValue, TPropertyType> binding)
            where TPropertyType : NDBasePropertyKey<TKey, TSourceType, TSourceValue>, INDProperty<TKey, TSourceType, TSourceValue>
            where TTargetType : class
            where TSourceType : class
        {
            var dictinary = this.table.GetOrCreateValue(binding.Target);
            dictinary[binding.Property] = binding;

            //NDProperty.Providers.LocalValueProvider<TKey>.Instance.AddEventHandler(binding.Property, binding.Target, LocalValueChanged);
        }

        private void LocalValueChanged<TType, TValue>(object sender, ChangedEventArgs<TKey, TType, TValue> e)
            where TType : class
        {

            //GetBinding(e.ChangedObject,e.)
        }

        internal void RemoveBinding<TSourceType, TSourceValue, TTargetType, TTargetValue, TPropertyType>(Binding<TKey, TSourceType, TSourceValue, TTargetType, TTargetValue, TPropertyType> binding)
            where TPropertyType : NDBasePropertyKey<TKey, TSourceType, TSourceValue>, INDProperty<TKey, TSourceType, TSourceValue>
            where TTargetType : class
            where TSourceType : class
        {
            Update(binding, default, false, () =>
            {
                if (this.table.TryGetValue(binding.Target, out var dictinary))
                {
                    dictinary.Remove(binding.Property);
                    if (dictinary.Count == 0)
                        this.table.Remove(binding.Target);
                }
                return true;
            });
        }

        //    public override void HigherProviderUpdated<TType, TValue, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue value, ValueProvider<TKey> updatedProvider)
        //        where TType : class
        //where TPropertyType : NDReadOnlyPropertyKey<TKey, TType, TValue>, INDProperty<TKey, TType, TValue>
        //    { }

        public override void HigherProviderUpdated<TType, TValue, TPropertyType>(object sender, TType targetObject, TPropertyType property, TValue value, ValueProvider<TKey> updatedProvider)
        {
            base.HigherProviderUpdated(sender, targetObject, property, value, updatedProvider);



        }



    }

    //internal sealed class Binding : IDisposable
    //{
    //    private object sourceProperty;
    //    private Action p;

    //    public Binding(object sourceProperty, Action p)
    //    {
    //        this.sourceProperty = sourceProperty;
    //        this.p = p;
    //    }

    //    public void Dispose()
    //    {
    //        p?.Invoke();
    //        p = null;
    //    }
    //}


}
