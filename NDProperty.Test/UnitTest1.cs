using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NDProperty.Propertys;

namespace NDProperty.Test
{
    [TestClass]
    public class UnitTest1
    {

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            // PropertyRegistar<Configuration>.Initilize(Providers.LocalValueManager.Instance, Providers.InheritenceValueManager.Instance, Providers.DefaultValueManager.Instance);
        }
        [TestMethod]
        public void TestSetAndGet()
        {
            var t1 = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";

            t1.Str = str1;
            Assert.AreEqual(str1, t1.Str);

            t1.Str = str2;
            Assert.AreEqual(str2, t1.Str);
        }
        [TestMethod]
        public void TestSetAndGetMultipleObjects()
        {
            var t1 = new TestObject();
            var t2 = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";

            t1.Str = str1;
            t2.Str = str2;
            Assert.AreEqual(str1, t1.Str);
            Assert.AreEqual(str2, t2.Str);
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
        public void TestSettingsCallOnChangedEquals()
        {
            var t = new TestObject();
            const string str1 = "Hallo Welt!";

            var p1 = PropertyRegistar<Configuration>.Register<string, TestObject>(x => x.TestChangeMethod, str1, NDPropertySettings.None);
            var p2 = PropertyRegistar<Configuration>.Register<string, TestObject>(x => x.TestChangeMethod, str1, NDPropertySettings.CallOnChangedHandlerOnEquals);


            PropertyRegistar<Configuration>.SetValue(p1, t, str1);
            t.testArguments = null;
            PropertyRegistar<Configuration>.SetValue(p1, t, str1);
            Assert.IsNull(t.testArguments);

            PropertyRegistar<Configuration>.SetValue(p2, t, str1);
            Assert.IsNotNull(t.testArguments);
            Assert.AreEqual(str1, t.testArguments.NewValue);
            //Assert.AreEqual(str1, t.testArguments.OldValue);
        }

        [TestMethod]
        public void TestReject()
        {
            var t = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";

            ChangedEventArgs<string, TestObject> eventArg = null;
            t.Str = str1;

            t.Reject = true;

            t.StrChanged += (sender, e) =>
            {
                eventArg = e;
            };

            t.Str = str2;

            Assert.IsNull(eventArg);
            Assert.AreEqual(str1, t.Str);
        }

        [TestMethod]
        public void TestRejectAndMutate()
        {
            var t = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";
            const string str3 = "Hallo Welt!3";

            ChangedEventArgs<string, TestObject> eventArg = null;
            t.Str = str1;

            t.Reject = true;
            t.Mutate = str3;

            t.StrChanged += (sender, e) =>
            {
                eventArg = e;
            };

            t.Str = str2;

            Assert.IsNull(eventArg);
            Assert.AreEqual(str1, t.Str);
        }

        [TestMethod]
        public void TestMutate()
        {
            var t = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";
            const string str3 = "Hallo Welt!3";

            ChangedEventArgs<string, TestObject> eventArg = null;
            t.Str = str1;

            t.Mutate = str3;

            t.StrChanged += (sender, e) =>
            {
                eventArg = e;
            };

            t.Str = str2;



            Assert.IsNotNull(eventArg);
            Assert.AreEqual(str3, eventArg.NewValue);
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


    public class Configuration
    {

    }

    public struct MyStruct
    {
        public static explicit operator MyStruct(int i)
        {
            return new MyStruct();
        }
    }

    public partial class TestObject
    {

        public bool Reject { get; set; }
        public string Mutate { get; set; }

        [NDP(Settigns = NDPropertySettings.CallOnChangedHandlerOnEquals)]
        //[System.ComponentModel.DefaultValue("asdf")]
        private void OnTestAttributeChanging(OnChangingArg<Configuration, MyStruct> arg)
        {
            var test = TestAttributeProperty.ToString();
        }

        [NDP(Settigns = NDPropertySettings.ReadOnly)]
        private void OnMyBlaChanging(OnChangingArg<Configuration, string> arg)
        {
            var test = TestAttributeProperty.ToString();
        }

        #region Attach
        public static readonly global::NDProperty.Propertys.NDAttachedPropertyKey<Configuration, string, object> AttachProperty = global::NDProperty.PropertyRegistar<Configuration>.RegisterAttached<string, object>(OnAttachChanged, default(string), global::NDProperty.Propertys.NDPropertySettings.None);

        public static global::NDProperty.Utils.AttachedHelper<Configuration, string, object> Attach { get; } = global::NDProperty.Utils.AttachedHelper.Create(AttachProperty);

        private static void OnAttachChanged(OnChangingArg<Configuration, string, object> arg)
        {

        }
        #endregion

        #region Str
        public static readonly NDPropertyKey<Configuration, string, TestObject> StrProperty = PropertyRegistar<Configuration>.Register<string, TestObject>(t => t.OnStrChanged, default(string), NDPropertySettings.None);

        public string Str
        {
            get { return PropertyRegistar<Configuration>.GetValue(StrProperty, this); }
            set { PropertyRegistar<Configuration>.SetValue(StrProperty, this, value); }
        }

        public event EventHandler<ChangedEventArgs<string, TestObject>> StrChanged
        {
            add { PropertyRegistar<Configuration>.AddEventHandler(StrProperty, this, value); }
            remove { PropertyRegistar<Configuration>.RemoveEventHandler(StrProperty, this, value); }
        }


        private void OnStrChanged(OnChangingArg<Configuration, string> arg)
        {
            arg.Reject = Reject;
            if (Mutate != null)
                arg.MutatedValue = Mutate;
        }
        #endregion

        #region InheritedStr
        public static readonly NDPropertyKey<Configuration, string, TestObject> InheritedStrProperty = PropertyRegistar<Configuration>.Register<string, TestObject>(t => t.OnInheritedStrChanged, default(string), NDPropertySettings.Inherited);

        public string InheritedStr
        {
            get => PropertyRegistar<Configuration>.GetValue(InheritedStrProperty, this);
            set => PropertyRegistar<Configuration>.SetValue(InheritedStrProperty, this, value);
        }

        public event EventHandler<ChangedEventArgs<string, TestObject>> InheritedStrChanged
        {
            add => PropertyRegistar<Configuration>.AddEventHandler(InheritedStrProperty, this, value);
            remove => PropertyRegistar<Configuration>.RemoveEventHandler(InheritedStrProperty, this, value);
        }

        private void OnInheritedStrChanged(OnChangingArg<Configuration, string> arg)
        {

        }
        #endregion


        #region Parent
        public static readonly NDPropertyKey<Configuration, TestObject, TestObject> ParentProperty = PropertyRegistar<Configuration>.Register<TestObject, TestObject>(t => t.OnParentChanged, default(TestObject), NDPropertySettings.ParentReference);
        public TestObject Parent
        {
            get => PropertyRegistar<Configuration>.GetValue(ParentProperty, this);
            set => PropertyRegistar<Configuration>.SetValue(ParentProperty, this, value);
        }

        public event EventHandler<ChangedEventArgs<TestObject, TestObject>> ParentChanged
        {
            add => PropertyRegistar<Configuration>.AddEventHandler(ParentProperty, this, value);
            remove => PropertyRegistar<Configuration>.RemoveEventHandler(ParentProperty, this, value);
        }

        private void OnParentChanged(OnChangingArg<Configuration, TestObject> arg)
        {

        }


        #endregion

        // BUg in Source code generator adds the triva at the end of the glass to the generated class. This leads to generating #endregion in the generated class. which produces an error.
        public override string ToString()
        {
            return base.ToString();
        }

        public OnChangingArg<Configuration, string> testArguments;

        internal void TestChangeMethod(OnChangingArg<Configuration, string> arg)
        {
            testArguments = arg;
        }
    }

}
