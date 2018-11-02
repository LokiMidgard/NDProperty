using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NDProperty.Propertys;
using NDProperty.Providers;
using NDProperty.Providers.Binding;


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

            ChangedEventArgs<Configuration, TestObject, string> eventArg = null;

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
        public void TestINotifyPropertyChanged()
        {
            var t = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";

            PropertyChangedEventArgs eventArg = null;

            t.PropertyChanged += (sender, e) =>
             {
                 eventArg = e;
             };

            t.NotifyTest = str1;

            Assert.IsNotNull(eventArg);
            Assert.AreEqual(nameof(t.NotifyTest), eventArg.PropertyName);
            eventArg = null;

            t.NotifyTest = str2;

            Assert.IsNotNull(eventArg);
            Assert.AreEqual(nameof(t.NotifyTest), eventArg.PropertyName);
        }


        [TestMethod]
        public void TestSettingsCallOnChangedEquals()
        {
            var t = new TestObject();
            const string str1 = "Hallo Welt!";

            var p1 = PropertyRegistar<Configuration>.Register<TestObject, string>(x => x.TestChangeMethod, str1, NDPropertySettings.None);
            var p2 = PropertyRegistar<Configuration>.Register<TestObject, string>(x => x.TestChangeMethod, str1, NDPropertySettings.CallOnChangedHandlerOnEquals);


            PropertyRegistar<Configuration>.SetValue(p1, t, str1);
            t.testArguments = null;
            PropertyRegistar<Configuration>.SetValue(p1, t, str1);
            Assert.IsNull(t.testArguments);

            PropertyRegistar<Configuration>.SetValue(p2, t, str1);
            Assert.IsNotNull(t.testArguments);
            Assert.AreEqual(str1, t.testArguments.Property.NewValue);
            //Assert.AreEqual(str1, t.testArguments.OldValue);
        }

        [TestMethod]
        public void TestReject()
        {
            var t = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";

            ChangedEventArgs<Configuration, TestObject, string> eventArg = null;
            t.Str = str1;

            try
            {

                t.Reject = true;

                t.StrChanged += (sender, e) =>
                {
                    eventArg = e;
                };

                t.Str = str2;

                Assert.IsNull(eventArg);
                Assert.AreEqual(str1, t.Str);
            }
            finally
            {
                t.Reject = false;
            }
        }



        [TestMethod]
        public void TestRejectAndMutate()
        {
            var t = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";
            const string str3 = "Hallo Welt!3";

            ChangedEventArgs<Configuration, TestObject, string> eventArg = null;
            t.Str = str1;

            t.Reject = true;
            try
            {
                t.Mutate = str3;

                t.StrChanged += (sender, e) =>
                {
                    eventArg = e;
                };

                t.Str = str2;

                Assert.IsNull(eventArg);
                Assert.AreEqual(str1, t.Str);
            }
            finally
            {
                t.Mutate = null;
            }
        }

        [TestMethod]
        public void TestMutate()
        {
            var t = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";
            const string str3 = "Hallo Welt!3";

            ChangedEventArgs<Configuration, TestObject, string> eventArg = null;
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
            Assert.AreEqual(str3, t.Str);
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

            ChangedEventArgs<Configuration, TestObject, string> eventArg = null;
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


            ChangedEventArgs<Configuration, TestObject, string> eventArg = null;
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


        [TestMethod]
        public void TestOneWayBinding()
        {
            var t1 = new TestObject();
            var t2 = new TestObject();
            const string str0 = "Hallo Welt!0";
            const string str1 = "Hallo Welt!1";
            const string str2 = "Hallo Welt!2";

            ChangedEventArgs<Configuration, TestObject, string> eventArg = null;

            t1.StrChanged += (sender, e) =>
            {
                eventArg = e;
            };
            t2.Str = str0;
            using (TestObject.StrProperty.Bind(t1, TestObject.StrProperty.Of(t2).OneWay()))
            {
                Assert.IsNotNull(eventArg);
                Assert.AreEqual(str0, t1.Str);
                Assert.AreEqual(str0, eventArg.NewValue);
                Assert.AreEqual(null, eventArg.OldValue);
                eventArg = null;

                t2.Str = str1;
                Assert.IsNotNull(eventArg);
                Assert.AreEqual(str1, t1.Str);
                Assert.AreEqual(str1, eventArg.NewValue);
                Assert.AreEqual(str0, eventArg.OldValue);
                eventArg = null;
            }

            // check if new value is notified if binding is cancled.
            Assert.IsNotNull(eventArg);
            Assert.AreEqual(null, t1.Str);
            Assert.AreEqual(null, eventArg.NewValue);
            Assert.AreEqual(str1, eventArg.OldValue);

            // check if binding is actual gone.
            eventArg = null;
            t2.Str = str2;

            Assert.IsNull(eventArg);
            Assert.AreEqual(null, t1.Str);
        }

        [TestMethod]
        public void TestOneWayBindingOver()
        {
            var t1 = new TestObject();
            var t2 = new TestObject();
            var t3 = new TestObject();
            var t4 = new TestObject();
            const string str1 = "Hallo 1";
            const string str2 = "Hallo 3";
            const string str3 = "Hallo 4";
            const string str4 = "Hallo 5";

            t3.Str = str2;
            t4.Str = str3;

            t2.Parent = t3;

            ChangedEventArgs<Configuration, TestObject, string> eventArg1 = null;
            ChangedEventArgs<Configuration, TestObject, string> eventArg2 = null;
            ChangedEventArgs<Configuration, TestObject, string> eventArg3 = null;
            ChangedEventArgs<Configuration, TestObject, string> eventArg4 = null;


            t1.StrChanged += (sender, e) =>
            {
                eventArg1 = e;
            };
            t2.StrChanged += (sender, e) =>
            {
                eventArg2 = e;
            };
            t3.StrChanged += (sender, e) =>
            {
                eventArg3 = e;
            };
            t4.StrChanged += (sender, e) =>
            {
                eventArg4 = e;
            };

            using (TestObject.StrProperty.Bind(t1, TestObject.ParentProperty.Of(t2).Over(TestObject.StrProperty).OneWay()))
            {
                Assert.IsNotNull(eventArg1);
                Assert.IsNull(eventArg2);
                Assert.IsNull(eventArg3);
                Assert.IsNull(eventArg4);

                Assert.AreEqual(str2, t1.Str);
                Assert.AreEqual(null, t2.Str);
                Assert.AreEqual(str2, t3.Str);
                Assert.AreEqual(str3, t4.Str);

                Assert.AreEqual(str2, eventArg1.NewValue);
                Assert.AreEqual(null, eventArg1.OldValue);
                eventArg1 = null;
                eventArg2 = null;
                eventArg3 = null;
                eventArg4 = null;

                t3.Str = str1;
                Assert.IsNotNull(eventArg1);
                Assert.IsNull(eventArg2);
                Assert.IsNotNull(eventArg3);
                Assert.IsNull(eventArg4);

                Assert.AreEqual(str1, t1.Str);
                Assert.AreEqual(null, t2.Str);
                Assert.AreEqual(str1, t3.Str);
                Assert.AreEqual(str3, t4.Str);

                Assert.AreEqual(str1, eventArg1.NewValue);
                Assert.AreEqual(str1, eventArg3.NewValue);
                Assert.AreEqual(str2, eventArg1.OldValue);
                Assert.AreEqual(str2, eventArg3.OldValue);
                eventArg1 = null;
                eventArg2 = null;
                eventArg3 = null;
                eventArg4 = null;

                t2.Parent = t4;
                Assert.IsNotNull(eventArg1);
                Assert.IsNull(eventArg2);
                Assert.IsNull(eventArg3);
                Assert.IsNull(eventArg4);

                Assert.AreEqual(str3, t1.Str);
                Assert.AreEqual(null, t2.Str);
                Assert.AreEqual(str1, t3.Str);
                Assert.AreEqual(str3, t4.Str);

                Assert.AreEqual(str3, eventArg1.NewValue);
                Assert.AreEqual(str1, eventArg1.OldValue);
                eventArg1 = null;
                eventArg2 = null;
                eventArg3 = null;
                eventArg4 = null;
            }

            // check if new value is notified if binding is cancled.
            Assert.IsNotNull(eventArg1);
            Assert.IsNull(eventArg2);
            Assert.IsNull(eventArg3);
            Assert.IsNull(eventArg4);

            Assert.AreEqual(null, t1.Str);
            Assert.AreEqual(null, t2.Str);
            Assert.AreEqual(str1, t3.Str);
            Assert.AreEqual(str3, t4.Str);

            Assert.AreEqual(null, eventArg1.NewValue);
            Assert.AreEqual(str3, eventArg1.OldValue);
            eventArg1 = null;
            eventArg2 = null;
            eventArg3 = null;
            eventArg4 = null;

            // check if binding is actual gone.
            t4.Str = str4;

            Assert.IsNull(eventArg1);
            Assert.AreEqual(null, t1.Str);
            Assert.AreEqual(null, t2.Str);
            Assert.AreEqual(str1, t3.Str);
            Assert.AreEqual(str4, t4.Str);
        }


        [TestMethod]
        public void TestTwoWayBinding()
        {
            var t1 = new TestObject();
            var t2 = new TestObject();
            const string str1 = "Hallo Welt!";
            const string str2 = "Hallo Welt!2";
            const string str3 = "Hallo Welt!3";

            ChangedEventArgs<Configuration, TestObject, string> eventArg1 = null;
            ChangedEventArgs<Configuration, TestObject, string> eventArg2 = null;


            t1.StrChanged += (sender, e) =>
            {
                eventArg1 = e;
            };
            t2.StrChanged += (sender, e) =>
            {
                eventArg2 = e;
            };

            using (TestObject.StrProperty.Bind(t1, TestObject.StrProperty.Of(t2).TwoWay()))
            {
                t1.Str = str1;
                Assert.IsNotNull(eventArg1);
                Assert.IsNotNull(eventArg2);
                Assert.AreEqual(str1, t2.Str);
                Assert.AreEqual(str1, t1.Str);
                Assert.AreEqual(str1, eventArg1.NewValue);
                Assert.AreEqual(str1, eventArg2.NewValue);
                Assert.AreEqual(null, eventArg1.OldValue);
                Assert.AreEqual(null, eventArg2.OldValue);
                eventArg1 = null;
                eventArg2 = null;

                t2.Str = str2;
                Assert.IsNotNull(eventArg1);
                Assert.IsNotNull(eventArg2);
                Assert.AreEqual(str2, t2.Str);
                Assert.AreEqual(str2, t1.Str);
                Assert.AreEqual(str2, eventArg1.NewValue);
                Assert.AreEqual(str1, eventArg1.OldValue);
                Assert.AreEqual(str2, eventArg2.NewValue);
                Assert.AreEqual(str1, eventArg2.OldValue);
                eventArg1 = null;
                eventArg2 = null;
            }

            // check if new value is notified if binding is cancled.
            Assert.IsNotNull(eventArg1);
            Assert.IsNull(eventArg2);
            Assert.AreEqual(str1, t1.Str);
            Assert.AreEqual(str2, t2.Str);
            Assert.AreEqual(str1, eventArg1.NewValue);
            Assert.AreEqual(str2, eventArg1.OldValue);

            // check if binding is actual gone.
            eventArg1 = null;
            eventArg2 = null;
            t2.Str = str3;

            Assert.IsNull(eventArg1);
            Assert.IsNotNull(eventArg2);
            Assert.AreEqual(str1, t1.Str);
            Assert.AreEqual(str3, t2.Str);
            Assert.AreEqual(str3, eventArg2.NewValue);
            Assert.AreEqual(str2, eventArg2.OldValue);

        }


    }


    public class Configuration : NDProperty.IInitializer<Configuration>
    {
        public IEnumerable<Providers.ValueProvider<Configuration>> ValueProvider => new Providers.ValueProvider<Configuration>[]
        {
            BindingProvider<Configuration>.Instance,
            LocalValueProvider<Configuration>.Instance,
            InheritenceValueProvider<Configuration>.Instance,

            DefaultValueProvider<Configuration>.Instance,
        };
    }

    public struct MyStruct
    {
        public static explicit operator MyStruct(int i)
        {
            return new MyStruct();
        }
    }

    public partial class TestObject : System.ComponentModel.INotifyPropertyChanged
    {

        public bool Reject { get; set; }
        public string Mutate { get; set; }

        [NDP(Settings = NDPropertySettings.CallOnChangedHandlerOnEquals | NDPropertySettings.ReadOnly)]
        //[System.ComponentModel.DefaultValue("asdf")]
        private void OnTestAttributeChanging(OnChangingArg<Configuration, MyStruct> arg)
        {
            var test = TestAttributeProperty.ToString();
        }

        [NDP(Settings = NDPropertySettings.ReadOnly)]
        [System.ComponentModel.DefaultValue("")]
        private void OnMyBlaChanging(OnChangingArg<Configuration, string> arg)
        {
            var test = TestAttributeProperty.ToString();
        }

        [NDP]
        private void OnNotifyTestChanging(OnChangingArg<Configuration, string> arg)
        {
        }


        #region Attach
        public static readonly global::NDProperty.Propertys.NDAttachedPropertyKey<Configuration, string, object> AttachProperty = global::NDProperty.PropertyRegistar<Configuration>.RegisterAttached<string, object>(OnAttachChanged, default(string), global::NDProperty.Propertys.NDPropertySettings.None);

        public static global::NDProperty.Utils.AttachedHelper<Configuration, string, object> Attach { get; } = global::NDProperty.Utils.AttachedHelper.Create(AttachProperty);

        private static void OnAttachChanged(OnChangingArg<Configuration, string, object> arg)
        {

        }
        #endregion

        #region Str
        public static readonly NDPropertyKey<Configuration, TestObject, string> StrProperty = PropertyRegistar<Configuration>.Register<TestObject, string>(t => t.OnStrChanged, default(string), NDPropertySettings.None);

        public string Str
        {
            get { return PropertyRegistar<Configuration>.GetValue(StrProperty, this); }
            set { PropertyRegistar<Configuration>.SetValue(StrProperty, this, value); }
        }

        public event EventHandler<ChangedEventArgs<Configuration, TestObject, string>> StrChanged
        {
            add { PropertyRegistar<Configuration>.AddEventHandler(StrProperty, this, value); }
            remove { PropertyRegistar<Configuration>.RemoveEventHandler(StrProperty, this, value); }
        }


        private void OnStrChanged(OnChangingArg<Configuration, string> arg)
        {
            if (Reject)
                arg.Provider.Reject = Reject;
            if (Mutate != null)
                arg.Provider.MutatedValue = Mutate;
        }
        #endregion

        #region InheritedStr
        public static readonly NDPropertyKey<Configuration, TestObject, string> InheritedStrProperty = PropertyRegistar<Configuration>.Register<TestObject, string>(t => t.OnInheritedStrChanged, default(string), NDPropertySettings.Inherited);

        public string InheritedStr
        {
            get => PropertyRegistar<Configuration>.GetValue(InheritedStrProperty, this);
            set => PropertyRegistar<Configuration>.SetValue(InheritedStrProperty, this, value);
        }

        public event EventHandler<ChangedEventArgs<Configuration, TestObject, string>> InheritedStrChanged
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

        public event EventHandler<ChangedEventArgs<Configuration, TestObject, TestObject>> ParentChanged
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

        public event PropertyChangedEventHandler PropertyChanged;

        internal void TestChangeMethod(OnChangingArg<Configuration, string> arg)
        {
            testArguments = arg;
        }
    }

}
