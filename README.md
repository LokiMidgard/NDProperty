[![Build status](https://ci.appveyor.com/api/projects/status/f2thpg6u4b3pb5p6?svg=true)](https://ci.appveyor.com/project/LokiMidgard/ndproperty)
[![NuGet](https://img.shields.io/nuget/v/NDProperty.svg?style=flat-square)](https://www.nuget.org/packages/NDProperty/)


# NDProperty

This Framework aims to provide simlar capabilitys as DependencyObjects. But with less boilerplate code thanks to code generation.

(NDProperty stands for **N**ot **D**ependency **Property**)

```c#
[NDP]
private void OnStrChanging(OnChangingArg<MyConfiguration, string> arg) { }
```

This is all that is needed for Propertys with getter setter events and everything else.

## Features

+ Implementing Propertys
+ Implementing ReadOnlyPropertys
+ No need to extend a class or implement an interface
+ Fires NotifyPropertyChanged if class support it (Currently not for Attached Propertys)
+ Callback with provides new and old value
+ Ability for an object to prevent a change
+ Support for Parent Child relationships of data objects
+ Inherit a value from a parent
+ Attached Propertys
+ Source code generator to easely implement this Propertys
+ Analizers and Codefixes to support this Framework
+ Default value (For compile time constants)
+ Binding (OneWay and TwoWay)
  + Between NDPropertys
+ Isolation to allow parrallel use
+ Modulised value resulution to allow custimisation

### Planed Features

+ Optimisation using WeakReferences
+ Default value (Using Generator)
+ Binding (OneWay and TwoWay)
  + Between NDProperty and POCO

### Things DependencyObjects have that this Framework will not support

+ Animation

## Getting Started

Install the [NDProperty Package](https://www.nuget.org/packages/NDProperty/) from NuGet. This will 
install the framwork, source code generator, code analyzer and codefix in your project.

To implement a simple property you have to ```Register``` this Property and implement a change handler. In the follwing sample the change handler is called ```OnStrChanging```. In addition a POCO property and eventhandler is implemented for convinience. The change handler validates the newValue and prevents the update 
of the Property if this value is not valid. The class must be partial

```c#
public partial class TestObject
{
    public static readonly NDPropertyKey<MyConfiguration, string, TestObject> StrProperty = PropertyRegistar<MyConfiguration>.Register<string, TestObject>(t => t.OnStrChanging, default(string), NDPropertySettings.None);

    public string Str
    {
        get { return PropertyRegistar<MyConfiguration>.GetValue(StrProperty, this); }
        set { PropertyRegistar<MyConfiguration>.SetValue(StrProperty, this, value); }
    }

    public event EventHandler<ChangedEventArgs<MyConfiguration, string, TestObject>> StrChanged
    {
        add { PropertyRegistar<MyConfiguration>.AddEventHandler(StrProperty, this, value); }
        remove { PropertyRegistar<MyConfiguration>.RemoveEventHandler(StrProperty, this, value); }
    }


    private void OnStrChanging(OnChangingArg<MyConfiguration, string> arg)
    {
        if (IsValid(arg.NewValue))
            arg.Reject = true;
    }
}

```

Using the Source Code Generator the boilerplate code is reduced. All that will need 
to be implemented is the change handler. The compiled code will be the same as above. 
**Important!** The Method must begin with ```On``` and end with ```Changing```. The 
Property name will be whatever is between this prä- and postfix. If this naming convention 
is not adhered this Property will not be generated.

```c#

[NDP]
private void OnStrChanging(OnChangingArg<MyConfiguration, string> arg)
{
    if (IsValid(arg.NewValue))
        arg.Reject = true;
}

```

The type ```MyConfiguration``` is called ConfigurationType and is used to isolate different instances of this libray. This allows side by side execution with different configurations ([see below](###Configuration)).

### Attached Propertys

Like with Dependency Propertys, you can define a Property that will be set on other Objects. 
In this case your change handler should be static and the argument must be of type 
```OnChangingArg<MyConfiguration, TValue, TType>```. Where ```TValue``` is the type of the value that can be set 
and ```TType``` the type of the objects where the value can be applied.

```c#
 private static readonly NDAttachedPropertyKey<MyConfiguration, string, MyOwnObject> StrProperty = PropertyRegistar<MyConfiguration>.RegisterAttached(OnAttChanging, default(string), NDPropertySettings.None);
       
private static void OnStrChanging(OnChangingArg<MyConfiguration, string, MyOwnObject> arg)
{
    if (IsValid(arg.NewValue))
        arg.Reject = true;
}
```
In the above example we can set a value of type ```string``` on every Object that is of the type of ```MyOwnObject``` or inherited this.


You can also use a Attribute to automaticly generate the property from the change handler and also generate some additional helper.

```c#
[NDPAttach]       
private static void OnStrChanging(OnChangingArg<MyConfiguration, string, MyOwnObject> arg)
{
    if (IsValid(arg.NewValue))
        arg.Reject = true;
}
```

This will generate follwing code:

```c#
public static readonly global::NDProperty.Propertys.NDAttachedPropertyKey<MyConfiguration, string, MyOwnObject> StrProperty = global::NDProperty.PropertyRegistar<MyConfiguration>.RegisterAttached<string, MyOwnObject>(OnStrChanging, false, global::NDProperty.Propertys.NDPropertySettings.None);
public static global::NDProperty.Utils.AttachedHelper<MyConfiguration, string, MyOwnObject> Str { get; } = global::NDProperty.Utils.AttachedHelper.Create(StrProperty);
```

The AttachedHelper provides access to the change event and allows to get and set the value on a 
specific instance. Assume the class ```MyClass``` Defines the Attached Property ```Str```. Then you 
could access the values of this property on every instance using the static Property ```Str``` on ```MyClass```


```c#
MyClass.Str[instance].Value = "Hallo Welt!";
MyClass.Str[instance].Changed += (object sender, ChangedEventArgs<string, object> e) => { };
Console.WriteLine(MyClass.Str[instance].Value); // => Hallo Welt!
```

### ReadOnly Property

To Implement a ReadOnlyProperty you can declare it like this:

```c#

private static readonly NDPropertyKey<MyConfiguration, string, TestObject> StrProperty = PropertyRegistar<MyConfiguration>.Register<string, TestObject>(t => t.OnStrChanging, NDPropertySettings.ReadOnly);
public static readonly NDReadOnlyPropertyKey<MyConfiguration, string, TestObject> StrReadOnlyProperty = StrProperty.ReadOnlyProperty;

```

The private field is used to set and get values, while the public field allows only reading the value.

The use of ```NDPropertySettings.ReadOnly``` does not have any effect on how the propertys can be used. You need to ensure that the access to the ```NDPropertyKey``` is restricted and only the ```NDReadOnlyPropertyKey``` has the public modifier. However the use of ```NDPropertySettings.ReadOnly``` instructs the code generator to generate code after the above pattern.

### Inherited

If your objects represent a tree you can inherit a value from it't parents. If a property is inherited 
and ```B``` is a child of ```A```, setting this property on ```A``` will alos set it on ```B```.

You can activate this behavior using the ```NDPropertySettings.Inherited``` setting on the ```PropertyRegistar.Register(...)``` method or on the Attributes.

In order to define the relation ship between objects, you should also define one Property as the parent reference.
This will be used to determin the structure of your tree. This can also be set using the Attributes or the ```Register``` methods and appling the setting ```NDPropertySettings.ParentReference```.

**Important!** If a property inhirets a value from its parent and this value is change, the EventHandler (```PropertyRegistar.AddEventHandler(...)```) of the Property is fired, but not the ```OnPropertyChanging``` method.

#### Null treatment

Normaly setting a value to null removes it from the Object. If this Property is inherited than it will have the value of its parent.
If this behavior is not desired, you can explicitly allow setting null. In this case setting the value to null in an object graph
will inherited the null value to the children.

Use ```NDPropertySettings.SetLocalExplicityNull``` to enable this.

### Default Vaule

You can set the default value of the Property using the ```System.ComponentModel.DefaultValueAttribute``` in code generation. The values that you can use are limited by what you can use as parameter for attributes.

If you manually register the Property there are no restrictions.

### Value resulution

A property can return different Values from different sources. Besides the directly set value on the object (the local value) there are two more sources that are implemented in this framework. Both are already discribed above in this docu. [Default Value](#default-value) and [Inherited](#Inherited).

In default configuration the order is following:
 + Local Value
 + Inherited Value
 + Default Value

If no local value was set, it will checked if an inherited value exists. If this is not the case the default Value will be returned (There is always a default value).

This functionality is provided by types extending ```ValueProvider<TKey>```. For the supplyed functionality these are:
+ ```LocalValueProvider<TKey>```
+ ```InheritanceValueProvider<TKey>```
+ ```DefaultValueProvider<TKey>```

You can create your own value provider by also extending the type ```ValueProvider<TKey>```. You have to implement the method and need to call Update whenever the value of this provider change. 

```c#
public abstract (TValue value, bool hasValue) GetValue<TValue, TType>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TKey, TValue, TType> property) where TType : class;
```

The ```Update``` method has an parameter ```updateCode```. This delegate should be updating your value. This is nessesary to safe the old value before the new value was set. It also has a Parameter that contains the new Value. You need to use this value instead of the one you are expecting to be the new value. This allows for the Mutated feature. If your Provider does not support mutation, than this value is always your new value.

To use your provider use it in the configuration type.

### Configuration

As mentioned above the configuration type is used to use different instances of this framework side by side. In order to change the default configuration this type must have an public parameterless constructor and implement the interface ```IInitializer<TKey>``` where ```TKey``` is the configuration Type.

An configuration equivalent to the default configuration would be following:

```c#

class MyConfiguration : IInitializer<MyConfiguration>
{
    public IEnumerable<ValueProvider<MyConfiguration>> ValueProviders {get;} new ValueProvider<MyConfiguration>[] {
        NDProperty.Providers.LocalValueProvider<MyConfiguration>.Instance,
        NDProperty.Providers.InheritanceValueProvider<MyConfiguration>.Instance,
        NDProperty.Providers.DefaultValueProvider<MyConfiguration>.Instance,
    };
}


```

## Callback Events

There are essential two kind of events. The Changed event everyone can subscrib to and the changing event that defines the property in the first place.

The simpler one is the changed event, this contains 4 Propertys.

- `NewValue`  
   This contains the new Value. It is already set on the Property.
- `OldValue`  
   This contains the Value the Property had before the change occured.
- `ChangedProperty`  
   This is the PropertyKey that changed.
- `ChangedObject`  
   This is the Object that was changed.

Every time the callback is called the Value of the Property has changed. The Changing event handler however is not quite that simple. It can be called without the Property actually changing. The reason for this is that the value can originate from different value providers (See [Value resulution](#Value-resulution)) and that the callback can prevent a change from happening.

If a provider changes its value, regardless if the actul property value is affacted, this callback is called. E.g. if you have a high provider that provides the value of an property, and a lower priorised provider changes, this callback will be called, but the value of the property will not change.

The event of this callback has 3 propertys:

- `ChangedObject`  
  This is a reference to the changing object.
- `Provider`  
  This encapsules all changes relevant for the provider.
- `Property`
  This encapsules all changes relevant for the Property.

If you want to check if a property actually changed, you can use `Property.IsObjectValueChanging`. If this value is `true` the other values can give you more informations on the state of the Propery. Those additional values are:

- `NewValue`  
  This contains the new Value that will be set.
- `OldValue`
  This contains the old, current, Value.
- `NewProvider`  
  This shows wich provider is responsible for the new value.
- `OldProvider`  
  This shows wich provider was responsible untill now for the value. If both are the same, the provider didn't change.

On the `Provider` property you have following propertys:

- `NewValue`  
  This contains the new Value that will be set on the provider. If a higher provider also provides a value, this will not be the new value of the Property (yet).
- `OldValue`
  This contains the old value of the provider. Again, this may not be the old value of the Property.
- `HasNewValue`  
  This tells if the provider will provide a value at all. If false `NewValue` has now mening.
- `HasOldValue`
  This tells if the provider had provide a value before. If false `OldValue` has now mening.
- `ChangingProvider`  
  Tells which provider is currently changing.
- `Reject`  
  Can be set to `true` in order to prevent this change from happening.
- `MutatedValue`  
  Can be set in order to change the Value that will be set. This can be used for example to trim a string.
- `CanChange`  
  If false the property `MutatedValue` and `Reject` mustn't be used.

The last member missing is the event `ExecuteAfterChange` on the event argument. This can be used to execute code after the change happend. This can be usfull for INotifyPropertyChanged interfaces. Not using this can sometimes result in endless rekursion, if your recursion anchor is a check on the value of this property. The Event will be fired after the change, but before other changed events are fired.


