using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NDProperty.Test
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestSetAndGet()
        {
            var t = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";

            t.Str = str1;
            Assert.AreEqual(str1, t.Str);

            t.Str = str2;
            Assert.AreEqual(str2, t.Str);
        }
        [TestMethod]
        public void TestListener()
        {
            var t = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";

            ChangedEventArgs<string, TestObject> eventArg = null;

            t.StrChanged += (sender, e) =>
            {
                eventArg = e;
            };

            t.Str = str1;

            Assert.IsNotNull(eventArg);
            Assert.AreEqual(str1, eventArg.NewValue);
            Assert.IsNull(eventArg.OldValue);
            Assert.AreSame(t, eventArg.ChangedObject);

            eventArg = null;

            t.Str = str2;

            Assert.IsNotNull(eventArg);
            Assert.AreEqual(str2, eventArg.NewValue);
            Assert.AreEqual(str1, eventArg.OldValue);
            Assert.AreSame(t, eventArg.ChangedObject);
        }

        [TestMethod]
        public void TestInheritedGet()
        {
            var tp = new TestObject();
            var tc = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";

            tc.Parent = tp;



            tp.InheritedStr = str1;
            Assert.AreEqual(str1, tc.InheritedStr);

            tp.InheritedStr = str2;
            Assert.AreEqual(str2, tc.InheritedStr);
        }

        [TestMethod]
        public void TestInheritedListener()
        {
            var tp = new TestObject();
            var tc = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";
            tc.Parent = tp;

            ChangedEventArgs<string, TestObject> eventArg = null;
            object eventSender = null;
            tc.InheritedStrChanged += (sender, e) =>
            {
                eventArg = e;
                eventSender = sender;
            };

            tp.InheritedStr = str1;

            Assert.IsNotNull(eventArg);
            Assert.AreEqual(str1, eventArg.NewValue);
            Assert.IsNull(eventArg.OldValue);
            Assert.AreSame(tc, eventArg.ChangedObject);
            Assert.AreSame(tp, eventSender);

            eventArg = null;
            eventSender = null;

            tp.InheritedStr = str2;

            Assert.IsNotNull(eventArg);
            Assert.AreEqual(str2, eventArg.NewValue);
            Assert.AreEqual(str1, eventArg.OldValue);
            Assert.AreSame(tp, eventSender);
        }


        [TestMethod]
        public void TestInheritedParentChanged()
        {
            var tp1 = new TestObject();
            var tp2 = new TestObject();
            var tc = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";
            tp1.InheritedStr = str1;
            tp2.InheritedStr = str2;


            ChangedEventArgs<string, TestObject> eventArg = null;
            object eventSender = null;
            tc.InheritedStrChanged += (sender, e) =>
            {
                eventArg = e;
                eventSender = sender;
            };

            tc.Parent = tp1;

            Assert.IsNotNull(eventArg);
            Assert.AreEqual(str1, eventArg.NewValue);
            Assert.IsNull(eventArg.OldValue);
            Assert.AreSame(tc, eventArg.ChangedObject);
            Assert.AreSame(tc, eventSender); //  sender is the chile because it changed it parent
            Assert.AreEqual(str1, tc.InheritedStr);

            eventArg = null;
            eventSender = null;

            tc.Parent = tp2;

            Assert.IsNotNull(eventArg);
            Assert.AreEqual(str2, eventArg.NewValue);
            Assert.AreEqual(str1, eventArg.OldValue);
            Assert.AreSame(tc, eventSender); //  sender is the chile because it changed it parent
            Assert.AreEqual(str2, tc.InheritedStr);

            // Check no Listener are fired if both parents had the same value
            eventArg = null;
            eventSender = null;

            tp1.InheritedStr = str2;
            tc.Parent = tp1;

            Assert.IsNull(eventArg);
            Assert.IsNull(eventSender);
            Assert.AreEqual(str2, tc.InheritedStr);

        }


    }

    public partial class TestObject
    {

        [NDP]
        private void OnTestAttributeChanged(global::NDProperty.OnChangedArg<string> arg)
        { 
            var test = TestAttributeProperty.ToString();
        }

        [NDP(IsReadOnly =true)]
        private void OnMyBlaChanged(OnChangedArg<string> arg)
        {
            var test = TestAttributeProperty.ToString();
        }

        #region Str
        public static readonly NDProperty<string, TestObject> StrProperty = PropertyRegistar.Register<string, TestObject>(t => t.OnStrChanged, false, NullTreatment.RemoveLocalValue,false);

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

        }
        #endregion

        #region InheritedStr
        public static readonly NDProperty<string, TestObject> InheritedStrProperty = PropertyRegistar.Register<string, TestObject>(t => t.OnInheritedStrChanged, true, NullTreatment.RemoveLocalValue,false);

        public string InheritedStr
        {
            get => PropertyRegistar.GetValue(InheritedStrProperty, this);
            set => PropertyRegistar.SetValue(InheritedStrProperty, this, value);
        }

        public event EventHandler<ChangedEventArgs<string, TestObject>> InheritedStrChanged
        {
            add => PropertyRegistar.AddEventHandler(InheritedStrProperty, this, value);
            remove => PropertyRegistar.RemoveEventHandler(InheritedStrProperty, this, value);
        }

        private void OnInheritedStrChanged(OnChangedArg<string> arg)
        {

        }
        #endregion


        #region Parent
        public static readonly NDProperty<TestObject, TestObject> ParentProperty = PropertyRegistar.Register<TestObject, TestObject>(t => t.OnParentChanged,false, NullTreatment.RemoveLocalValue, true);
        public TestObject Parent
        {
            get => PropertyRegistar.GetValue(ParentProperty, this);
            set => PropertyRegistar.SetValue(ParentProperty, this, value);
        }

        public event EventHandler<ChangedEventArgs<TestObject, TestObject>> ParentChanged
        {
            add => PropertyRegistar.AddEventHandler(ParentProperty, this, value);
            remove => PropertyRegistar.RemoveEventHandler(ParentProperty, this, value);
        }

        private void OnParentChanged(OnChangedArg<TestObject> arg)
        {

        }


        #endregion

        // BUg in Source code generator adds the triva at the end of the glass to the generated class. This leads to generating #endregion in the generated class. which produces an error.
        public override string ToString()
        {
            return base.ToString();
        }
    }

}
