using ASCompletion.Completion;
using ASCompletion.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginCore.Utilities;
using System.IO;

namespace ASCompletion.Test.Context
{
    [TestClass]
    public class ASContextTest
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
        public void TestFirstRun()
        {
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            Assert.IsNull(context.Settings);
            Assert.IsNotNull(context.Features);
            Assert.AreEqual(0, context.CurrentLine);
            Assert.IsNull(context.CurrentMember);
            Assert.AreEqual(ClassModel.VoidClass, context.CurrentClass);
            Assert.IsTrue(string.IsNullOrEmpty(context.CurrentFile));
            Assert.IsNotNull(context.CurrentModel);
            Assert.IsFalse(context.InPrivateSection);
            Assert.IsFalse(context.IsFileValid);
            Assert.IsFalse(context.CanBuild);
            Assert.IsNull(context.TopLevel);
            Assert.IsNull(context.Classpath);
        }

        [TestMethod]
        public void TestGetModel()
        {
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            Assert.AreEqual(ClassModel.VoidClass, context.GetModel("", "", ""));
            Assert.AreEqual(ClassModel.VoidClass, context.GetModel("flashdevelop", "TestClass", ""));
            Assert.AreEqual(ClassModel.VoidClass, context.GetModel("flashdevelop", "TestClass", "tests"));
        }

        [TestMethod]
        public void TestCreateFileModelForEmptyFileName()
        {
            string fileName = string.Empty;
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            FileModel aFile = context.CreateFileModel(fileName);
            Assert.IsNotNull(aFile);
            Assert.AreEqual(fileName, aFile.FileName);
            Assert.IsNull(aFile.Context);
        }

        [TestMethod]
        public void TestCreateFileModelForNullFileName()
        {
            string fileName = null;
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            FileModel aFile = context.CreateFileModel(fileName);
            Assert.IsNotNull(aFile);
            Assert.IsTrue(string.IsNullOrEmpty(aFile.FileName));
            Assert.IsNull(aFile.Context);
        }

        [TestMethod]
        public void TestCreateFileModelForValidFile()
        {
            string fileName = Path.GetFullPath(PathHelper.as3FileWithUserObjectClass);
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            FileModel aFile = context.CreateFileModel(fileName);
            Assert.IsNotNull(aFile);
            Assert.AreEqual(fileName, aFile.FileName);
            Assert.AreEqual(context, aFile.Context);
        }

        [TestMethod]
        public void TestGetFileModelForEmptyFileName()
        {
            string fileName = string.Empty;
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            FileModel aFile = context.GetFileModel(fileName);
            Assert.AreEqual(0, aFile.Version);
        }

        [TestMethod]
        public void TestGetFileModelForNullFileName()
        {
            string fileName = null;
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            FileModel aFile = context.GetFileModel(fileName);
            Assert.AreEqual(0, aFile.Version);
        }

        [TestMethod]
        public void TestGetFileModelForValidFile()
        {
            string fileName = Path.GetFullPath(PathHelper.as3FileWithUserObjectClass);
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            FileModel aFile = context.GetFileModel(fileName);
            Assert.AreEqual(3, aFile.Version);
        }

        [TestMethod]
        public void TestGetCachedFileModelForEmptyFileName()
        {
            string fileName = string.Empty;
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            FileModel aFile = context.GetCachedFileModel(fileName);
            Assert.IsNotNull(aFile);
            Assert.IsTrue(string.IsNullOrEmpty(aFile.FileName));
            Assert.AreEqual(0, aFile.Version);
        }

        [TestMethod]
        public void TestGetCachedFileModelForNullFileName()
        {
            string fileName = null;
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            FileModel aFile = context.GetCachedFileModel(fileName);
            Assert.IsNotNull(aFile);
            Assert.IsTrue(string.IsNullOrEmpty(aFile.FileName));
            Assert.AreEqual(0, aFile.Version);
        }

        [TestMethod]
        public void TestGetCachedFileModelForValidFile()
        {
            string fileName = Path.GetFullPath(PathHelper.as3FileWithUserObjectClass);
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            FileModel aFile = context.GetCachedFileModel(fileName);
            Assert.IsNotNull(aFile);
            Assert.AreEqual(fileName, aFile.FileName);
            Assert.AreEqual(3, aFile.Version);
        }

        [TestMethod]
        public void TestFilterSource()
        {
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            Assert.AreEqual("src", context.FilterSource(string.Empty, "src"));
        }

        [TestMethod]
        public void TestIsModelValidForNullFileModel()
        {
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            Assert.IsFalse(context.IsModelValid(null, null));
        }

        [TestMethod]
        public void TestIsModelValidForSomeFileModel()
        {
            string fileName = Path.GetFullPath(PathHelper.as3FileWithUserObjectClass);
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            FileModel aFile = context.GetFileModel(fileName);
            Assert.IsTrue(context.IsModelValid(aFile, null));
        }

        [TestMethod]
        public void TestGetDeclarationAtLine0ForEmptyContext()
        {
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            context.CurrentFile = null;
            ASResult result = context.GetDeclarationAtLine(0);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsNull());
        }

        [TestMethod]
        public void TestGetDeclarationAtLine0ForAS3File()
        {
            ASCompletion.Context.ASContext context = (ASCompletion.Context.ASContext)ASCompletion.Context.ASContext.Context;
            context.CurrentFile = Path.GetFullPath(PathHelper.as3FileWithUserObjectClass);
            ASResult result = context.GetDeclarationAtLine(0);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsNull());
        }
    }
}