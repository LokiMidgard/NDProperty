[![Build status](https://ci.appveyor.com/api/projects/status/f2thpg6u4b3pb5p6?svg=true)](https://ci.appveyor.com/project/LokiMidgard/ndproperty)
[![NuGet](https://img.shields.io/nuget/v/NDProperty.svg?style=flat-square)](https://www.nuget.org/packages/NDProperty/)


# NDProperty

This Framework aims to provide simlar capabilitys as DependencyObjects. But with less boilerplate code thanks to code generation.

(NDProperty stands for **N**ot **D**ependency **Property**)

```c#
[NDP]
private void OnStrChanged(OnChangedArg<string> arg) { }
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

### Planed Features

+ Binding
+ Optimisation using WeakReferences
+ Default value (Using Generator)

### Things DependencyObjects have that this Framework will not support

+ Animation

## Getting Started

Install the [NDProperty Package](https://www.nuget.org/packages/NDProperty/) from NuGet. This will 
install the framwork, source code generator, code analyzer and codefix in your project.

To implement a simple property you have to ```Register``` this Property and implement a change handler. 
In the follwing sample the change handler is called ```OnStrChanged```. In addition a POCO property and 
eventhandler is implemented for convinience. The change handler validates the newValue and prevents the update 
of the Property if this value is not valid. The class must be partial

```c#
public partial class TestObject
{
    public static readonly NDProperty<string, TestObject> StrProperty = PropertyRegistar.Register<string, TestObject>(t => t.OnStrChanged, default(string), false, NullTreatment.RemoveLocalValue);

    public string Str
    {
        get { return PropertyRegistar.GetValue(StrProperty, this); }
        set { PropertyRegistar.SetValue(StrProperty, this, value); }
    }

    public event EventHandler<ChangedEventArgs<string, TestObject>> StrChanged
    {
        add { PropertyRegistar.AddEventHandler(StrProperty, this, value); }
        remove { PropertyRegistar.RemoveEventHandler(StrProperty, this, value); }
    }


    private void OnStrChanged(OnChangedArg<string> arg)
    {
        if (IsValid(arg.NewValue))
            arg.Reject = true;
    }
}

```

Using the Source Code Generator the boilerplate code is reduced. All that will need 
to be implemented is the change handler. The compiled code will be the same as above. 
**Important!** The Method must begin with ```On``` and end with ```Changed```. The 
Property name will be whatever is between this prä- and postfix. If this naming convention 
is not adhered this Property will not be generated.

```c#

[NDP]
private void OnStrChanged(OnChangedArg<string> arg)
{
    if (IsValid(arg.NewValue))
        arg.Reject = true;
}

```

### Attached Propertys

Like with Dependency Propertys, you can define a Property that will be set on other Objects. 
In this case your change handler should be static and the argument must be of type 
```OnChangedArg<TValue, TType>```. Where ```TValue``` is the type of the value that can be set 
and ```TType``` the type of the objects where the value can be applied.

```c#
 private static readonly global::NDProperty.NDAttachedProperty<string, MyOwnObject> StrProperty = global::NDProperty.PropertyRegistar.RegisterAttached<string, MyOwnObject>(OnAttChanged, default(string), false, global::NDProperty.NullTreatment.RemoveLocalValue, false);
       
private static void OnStrChanged(OnChangedArg<string, MyOwnObject> arg)
{
    if (IsValid(arg.NewValue))
        arg.Reject = true;
}
```
In the above example we can set a value of type ```string``` on every Object that is of the type of ```MyOwnObject``` or inherited this.


You can also use a Attribute to automaticly generate the property from the change handler and also generate some additional helper.

```c#
[NDPAttach]       
private static void OnStrChanged(OnChangedArg<string, MyOwnObject> arg)
{
    if (IsValid(arg.NewValue))
        arg.Reject = true;
}
```

This will generate follwing code:

```c#
public static readonly global::NDProperty.NDAttachedProperty<string, MyOwnObject> StrProperty = global::NDProperty.PropertyRegistar.RegisterAttached<string, MyOwnObject>(OnStrChanged, false, global::NDProperty.NullTreatment.RemoveLocalValue, false);
public static global::NDProperty.PropertyRegistar.AttachedHelper<string, MyOwnObject> Str { get; } = global::NDProperty.PropertyRegistar.AttachedHelper.Create(StrProperty);
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

private static readonly NDProperty<string, TestObject> StrProperty = PropertyRegistar.Register<string, TestObject>(t => t.OnStrChanged, false, NullTreatment.RemoveLocalValue);
public static readonly NDReadOnlyProperty<string, TestObject> StrReadOnlyProperty = StrProperty.ReadOnlyProperty;

```

The private field is used to set and get values, while the public field allows only reading the value.
You can set the property ```IsReadOnly``` on one of the above mentioned attributes to instruct the generator 
to create readonly NDPropertys.

### Inherited

If your objects represent a tree you can inherit a value from it't parents. If a property is inherited 
and ```B``` is a child of ```A```, setting this property on ```A``` will alos set it on ```B```.

You can activate this behavior using the ```Register``` methods on the ```PropertyRegistar``` or using the Attributes.

In order to define the relation ship between objects, you should also define one Proeprty as the parent reference.
This will be used to determin the structure of your tree. This can also be set using the Attributes or the ```Register``` methods.

#### Null treatment

Normaly setting a value to null removes it from the Object. If this Property is inherited than it will have the value of its parent.
If this behavior is not desired, you can explicitly allow setting null. In this case setting the value to null in an object graph
will inherited the null value to the children.

### Default Vaule

You can set the default value of the Property using the ```System.ComponentModel.DefaultValueAttribute```. The values that you can use are limited by what you can use as parameter for attributes.