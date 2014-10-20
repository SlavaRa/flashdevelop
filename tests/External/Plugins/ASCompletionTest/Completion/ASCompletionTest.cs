using Microsoft.VisualStudio.TestTools.UnitTesting;
using ASCompletion;
using PluginCore.Utilities;
using PluginCore;
using FlashDevelop.Mock.Docking;
using ASCompletion.Completion;

namespace ASCompletionTest.Completion
{
    [TestClass]
    public class ASCompletionTest
    {
        [TestInitialize]
        public void Initialize()
        {
            FlashDevelop.Mock.MainForm mainForm = new FlashDevelop.Mock.MainForm();
            SingleInstanceApp.Initialize();
            PluginMain pluginMain = new ASCompletion.PluginMain();
            pluginMain.Initialize();
        }

        [TestCleanup]
        public void Cleanup()
        {
            SingleInstanceApp.Close();
        }

        [TestMethod]
        public void TestFindParameterIndex()
        {
            string fileName = ASCompletion.Test.PathHelper.as3AS3TestFindParameterIndex;
            TabbedDocument document = (TabbedDocument)PluginBase.MainForm.OpenEditableDocument(fileName);
            int position = 179;
            Assert.AreEqual(0, ASComplete.FindParameterIndex(document.SciControl, ref position));
            position = 193;
            Assert.AreEqual(1, ASComplete.FindParameterIndex(document.SciControl, ref position));
            position = 208;
            Assert.AreEqual(2, ASComplete.FindParameterIndex(document.SciControl, ref position));
            position = 221;
            Assert.AreEqual(0, ASComplete.FindParameterIndex(document.SciControl, ref position));
            position = 235;
            Assert.AreEqual(1, ASComplete.FindParameterIndex(document.SciControl, ref position));
        }
    }
}