[![Build status](https://ci.appveyor.com/api/projects/status/f2thpg6u4b3pb5p6?svg=true)](https://ci.appveyor.com/project/LokiMidgard/ndproperty)
[![NuGet](https://img.shields.io/nuget/v/NDProperty.svg?style=flat-square)](https://www.nuget.org/packages/NDProperty/)


# NDProperty

This Framework aims to provide simlar capabilitys as DependencyObjects. But with less boilerplate code thanks to code generation.

(NDProperty stands for **N**ot **D**ependency **Property**)

```c#
[NDP]
private void OnStrChanging(OnChangingArg<MyClass, string> arg) { }
```

This is all that is needed for Propertys with getter setter events and everything else.

## Features

+ Implementing Propertys
+ Implementing ReadOnlyPropertys
+ No need to extend a class or implement an interface
+ Callback with provides new and old value
+ Ability for an object to prevent a change
+ Providing error text for invalid values (Through cant be readed yet :/)
+ Support for Parent Child relationships of data objects
+ Inherit a value from a parent
+ Attached Propertys
+ Source code generator to easely implement this Propertys
+ Analizers and Codefixes to support this Framework
+ Default value (For compile time constants)
+ Binding (OneWay and TwoWay)
  + Between NDPropertys
  + Between NDProperty and POCO
+ Isolation to allow parrallel use
+ Modulised value resulution to allow custimisation

### Planed Features

+ Optimisation using WeakReferences
+ Default value (Using Generator)

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
    public static readonly NDPropertyKey<MyClass, string, TestObject> StrProperty = PropertyRegistar<MyClass>.Register<string, TestObject>(t => t.OnStrChanging, default(string), NDPropertySettings.None);

    public string Str
    {
        get { return PropertyRegistar<MyClass>.GetValue(StrProperty, this); }
        set { PropertyRegistar<MyClass>.SetValue(StrProperty, this, value); }
    }

    public event EventHandler<ChangedEventArgs<MyClass, string, TestObject>> StrChanged
    {
        add { PropertyRegistar<MyClass>.AddEventHandler(StrProperty, this, value); }
        remove { PropertyRegistar<MyClass>.RemoveEventHandler(StrProperty, this, value); }
    }


    private void OnStrChanging(OnChangingArg<MyClass, string> arg)
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
private void OnStrChanging(OnChangingArg<MyClass, string> arg)
{
    if (IsValid(arg.NewValue))
        arg.Reject = true;
}

```

The type ```MyClass``` is called ConfigurationType and is used to isolate different instances of this libray. This allows side by side execution with different configurations ([see below](###Configuration)).

### Attached Propertys

Like with Dependency Propertys, you can define a Property that will be set on other Objects. 
In this case your change handler should be static and the argument must be of type 
```OnChangingArg<MyClass, TValue, TType>```. Where ```TValue``` is the type of the value that can be set 
and ```TType``` the type of the objects where the value can be applied.

```c#
 private static readonly NDAttachedPropertyKey<MyClass, string, MyOwnObject> StrProperty = PropertyRegistar<MyClass>.RegisterAttached(OnAttChanging, default(string), NDPropertySettings.None);
       
private static void OnStrChanging(OnChangingArg<MyClass, string, MyOwnObject> arg)
{
    if (IsValid(arg.NewValue))
        arg.Reject = true;
}
```
In the above example we can set a value of type ```string``` on every Object that is of the type of ```MyOwnObject``` or inherited this.


You can also use a Attribute to automaticly generate the property from the change handler and also generate some additional helper.

```c#
[NDPAttach]       
private static void OnStrChanging(OnChangingArg<MyClass, string, MyOwnObject> arg)
{
    if (IsValid(arg.NewValue))
        arg.Reject = true;
}
```

This will generate follwing code:

```c#
public static readonly global::NDProperty.Propertys.NDAttachedPropertyKey<MyClass, string, MyOwnObject> StrProperty = global::NDProperty.PropertyRegistar<MyClass>.RegisterAttached<string, MyOwnObject>(OnStrChanging, false, global::NDProperty.Propertys.NDPropertySettings.None);
public static global::NDProperty.Utils.AttachedHelper<MyClass, string, MyOwnObject> Str { get; } = global::NDProperty.Utils.AttachedHelper.Create(StrProperty);
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

private static readonly NDPropertyKey<MyClass, string, TestObject> StrProperty = PropertyRegistar<MyClass>.Register<string, TestObject>(t => t.OnStrChanging, NDPropertySettings.ReadOnly);
public static readonly NDReadOnlyPropertyKey<MyClass, string, TestObject> StrReadOnlyProperty = StrProperty.ReadOnlyProperty;

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
+ ```InheritenceValueProvider<TKey>```
+ ```DefaultValueProvider<TKey>```

You can create your own value provider by also extending the type ```ValueProvider<TKey>```. You have to implement the method and need to call Update whenever the value of this provider change. 

```c#
public abstract (TValue value, bool hasValue) GetValue<TValue, TType>(TType targetObject, Propertys.NDReadOnlyPropertyKey<TKey, TValue, TType> property) where TType : class;
```

The ```Update``` method has an parameter ```updateCode```. This delegate should be updating your value. This is nessesary to safe the old value before the new value was set.

Alternativly you can provide the old value and the provider that provided the value.

To use your provider use it in the configuration type.

### Configuration

As mentioned above the configuration type is used to use different instances of this framework side by side. In order to change the default configuration this type must have an public parameterless constructor and implement the interface ```IInitilizer<TKey>``` where ```TKey``` is the configuration Type.

An configuration equivalent to the default configuration would be following:

```c#

class MyConfiguration : IInitilizer<MyConfiguration>
{
    public IEnumerable<ValueProvider<MyConfiguration>> ValueProvider => new ValueProvider<MyConfiguration>[] {
        NDProperty.Providers.LocalValueProvider<MyConfiguration>.Instance,
        NDProperty.Providers.InheritenceValueProvider<MyConfiguration>.Instance,
        NDProperty.Providers.DefaultValueProvider<MyConfiguration>.Instance,
    };
}


```