namespace NDProperty.Propertys
{
    internal interface IInternalNDReadOnlyProperty
    {

        object GetValue(object obj);
        object GetLocalValue(object obj);
        bool HasLocalValue(object obj);
        void CallChangeHandler(object obj, object sender, object oldValue, object newValue);
    }


}
