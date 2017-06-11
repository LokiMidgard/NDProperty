# NDProperty

This Framework aims to provide simlar capabilitys as DependencyObjects. But with less boilerplate code thanks to code generation.

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

## Planed Features

+ Binding
+ Analizers and Codefixes to support this Framework
+ Optimisation using WeakReferences

## Things DependencyObjects have that this Framework will not support

+ Animation

# Getting Started

To implement a simple property you have to ```Register``` this Property and implement a change handler. In the follwing sample the change handler is called ```OnStrChanged```. In addition a POCO property and eventhandler is implemented for convinience. The change handler validates the newValue and prevents the update of the Property if this value is not valid. The class must be partial

```c#
public partial class TestObject
{
    public static readonly NDProperty<string, TestObject> StrProperty = PropertyRegistar.Register<string, TestObject>(t => t.OnStrChanged, false, NullTreatment.RemoveLocalValue);

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

Using the Source Code Generator the boilerplate code is reduced. All that will need to be implemented is the change handler. The compiled code will be the same as above. **Important!** The Method must begin with ```On``` and end with ```Changed```. The Property name will be whatever is between this prä- and postfix. If this naming convention is not adhered this Property will not be generated.

```c#

[NDP]
private void OnStrChanged(OnChangedArg<string> arg)
{
    if (IsValid(arg.NewValue))
        arg.Reject = true;
}

```

To Implement a ReadOnlyProperty you can declare it like this:

```c#

private static readonly NDProperty<string, TestObject> StrProperty = PropertyRegistar.Register<string, TestObject>(t => t.OnStrChanged, false, NullTreatment.RemoveLocalValue);
public static readonly NDReadOnlyProperty<string, TestObject> StrProperty2 = StrProperty.ReadOnlyProperty;

public string Str
{
    get { return PropertyRegistar.GetValue(StrProperty, this); }
    private set { PropertyRegistar.SetValue(StrProperty, this, value); }
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

```

To use inherit you have to put your data objects in a parent child relationship. You do this in implementing a Parent Property on your objects. You then use the ```RegisterParent(...)``` method to create the parent child relation ship. A Parent can be a different type then the child.

For attached propertys you use the ```RegisterAttached(...)``` Method. 

Currently CodeGeneration is only supported for normal Propertys Register. The other Register methods will follow soon.

A NuGet package will follwo as soon I have checked the generator will work deplyed using nuget.