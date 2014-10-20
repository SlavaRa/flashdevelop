using FlashDevelop.Dialogs;
using FlashDevelop.Helpers;
using FlashDevelop.Managers;
using FlashDevelop.Mock.Docking;
using FlashDevelop.Mock.Managers;
using FlashDevelop.Settings;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Localization;
using PluginCore.Managers;
using PluginCore.Utilities;
using ScintillaNet.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FlashDevelop.Mock
{
    public class MainForm : Form, IMainForm, IMessageFilter
    {
        public static MainForm Instance;
        private DockPanel dockPanel;
        private ToolStrip toolStrip;
        private FRInFilesDialog frInFilesDialog;
        private SettingObject appSettings;
        private ContextMenuStrip editorMenu;
        private bool isFullScreen = false;

        public MainForm()
        {
            Type type = typeof(PluginBase);
            MemberInfo member = type.GetMember("instance", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static)[0];
            ((FieldInfo)member).SetValue(null, this);//PluginBase.Initialize(this);
            InitializeSettings();
            dockPanel = new DockPanel();
            Instance = this;
        }

        /// <summary>
        /// Initializes the application settings
        /// </summary>
        private void InitializeSettings()
        {
            this.appSettings = SettingObject.GetDefaultSettings();
            if (File.Exists(FileNameHelper.SettingData))
            {
                Object obj = ObjectSerializer.Deserialize(FileNameHelper.SettingData, this.appSettings, false);
                this.appSettings = (SettingObject)obj;
            }
            SettingObject.EnsureValidity(this.appSettings);
            FileStateManager.RemoveOldStateFiles();
        }

        /// <summary>
        /// Stop the currently running process
        /// </summary>
        public void KillProcess()
        {   
        }

        public void RefreshUI()
        {
            throw new NotImplementedException();
        }

        public void RefreshSciConfig()
        {
            throw new NotImplementedException();
        }

        public void ClearTemporaryFiles(string file)
        {
            throw new NotImplementedException();
        }

        public void ShowSettingsDialog(string itemName)
        {
            throw new NotImplementedException();
        }

        public void ShowErrorDialog(object sender, Exception ex)
        {
            Debug.WriteLine(ex);
        }

        public void ShowSettingsDialog(string itemName, string filter)
        {
            throw new NotImplementedException();
        }

        public void AutoUpdateMenuItem(ToolStripItem item, string action)
        {
            throw new NotImplementedException();
        }

        public void RegisterShortcutItem(string id, Keys keys)
        {
        }

        public void RegisterShortcutItem(string id, ToolStripMenuItem item)
        {
        }

        public void FileFromTemplate(string templatePath, string newFilePath)
        {
            throw new NotImplementedException();
        }

        public DockContent OpenEditableDocument(string org, Encoding encoding, bool restorePosition)
        {
            DockContent createdDoc;
            EncodingFileInfo info;
            string file = PathHelper.GetPhysicalPathName(org);
            TextEvent te = new TextEvent(EventType.FileOpening, file);
            EventManager.DispatchEvent(this, te);
            if (te.Handled)
            {
                if (this.Documents.Length == 0)
                {
                    this.New(null, null);
                    return null;
                }
                else return null;
            }
            else if (file.EndsWith(".delete.fdz"))
            {
                this.CallCommand("RemoveZip", file);
                return null;
            }
            else if (file.EndsWith(".fdz"))
            {
                this.CallCommand("ExtractZip", file);
                if (file.ToLower().IndexOf("theme") != -1)
                {
                    string currentTheme = Path.Combine(PathHelper.ThemesDir, "CURRENT");
                    //if (File.Exists(currentTheme)) ThemeManager.LoadTheme(currentTheme);
                    this.RefreshSciConfig();
                    //this.Refresh();
                }
                return null;
            }
            try
            {
                int count = this.Documents.Length;
                for (int i = 0; i < count; i++)
                {
                    if (this.Documents[i].IsEditable && this.Documents[i].FileName.ToUpper() == file.ToUpper())
                    {
                        this.Documents[i].Activate();
                        return null;
                    }
                }
            }
            catch { }
            if (encoding == null)
            {
                info = FileHelper.GetEncodingFileInfo(file);
                if (info.CodePage == -1) return null; // If the file is locked, stop.
            }
            else
            {
                info = FileHelper.GetEncodingFileInfo(file);
                if (info.CodePage == -1) return null; // If the file is locked, stop.
                info.Contents = FileHelper.ReadFile(file, encoding);
                info.CodePage = encoding.CodePage;
            }
            DataEvent de = new DataEvent(EventType.FileDecode, file, null);
            EventManager.DispatchEvent(this, de); // Lets ask if a plugin wants to decode the data..
            if (de.Handled)
            {
                info.Contents = de.Data as String;
                info.CodePage = Encoding.UTF8.CodePage; // assume plugin always return UTF8
            }
            try
            {
                if (this.CurrentDocument != null && this.CurrentDocument.IsUntitled && !this.CurrentDocument.IsModified && this.Documents.Length == 1)
                {
                    //this.closingForOpenFile = true;
                    this.CurrentDocument.Close();
                    //this.closingForOpenFile = false;
                    createdDoc = this.CreateEditableDocument(file, info.Contents, info.CodePage);
                }
                else createdDoc = this.CreateEditableDocument(file, info.Contents, info.CodePage);
                //ButtonManager.AddNewReopenMenuItem(file);
            }
            catch
            {
                createdDoc = this.CreateEditableDocument(file, info.Contents, info.CodePage);
                //ButtonManager.AddNewReopenMenuItem(file);
            }
            FlashDevelop.Mock.Docking.TabbedDocument document = (FlashDevelop.Mock.Docking.TabbedDocument)createdDoc;
            document.SciControl.SaveBOM = info.ContainsBOM;
            document.SciControl.BeginInvoke((MethodInvoker)delegate
            {
                //if (this.appSettings.RestoreFileStates)
                //{
                //    FileStateManager.ApplyFileState(document, restorePosition);
                //}
            });
            //ButtonManager.UpdateFlaggedButtons();
            return createdDoc;
        }
        public DockContent OpenEditableDocument(string file, bool restorePosition)
        {
            return this.OpenEditableDocument(file, null, restorePosition);
        }
        public DockContent OpenEditableDocument(string file)
        {
            return this.OpenEditableDocument(file, null, true);
        }

        /// <summary>
        /// Creates a new custom document
        /// </summary>
        public DockContent CreateCustomDocument(Control ctrl)
        {
            TabbedDocument tabbedDocument = new TabbedDocument();
            //tabbedDocument.Closing += new System.ComponentModel.CancelEventHandler(this.OnDocumentClosing);
            //tabbedDocument.Closed += new System.EventHandler(this.OnDocumentClosed);
            tabbedDocument.Text = TextHelper.GetString("Title.CustomDocument");
            //tabbedDocument.TabPageContextMenuStrip = this.tabMenu;
            tabbedDocument.Controls.Add(ctrl);
            tabbedDocument.Show();
            return tabbedDocument;
        }

        /// <summary>
        /// Creates a new empty document
        /// </summary>
        public DockContent CreateEditableDocument(string file, string text, int codepage)
        {
            //this.notifyOpenFile = true;
            FlashDevelop.Mock.Docking.TabbedDocument tabbedDocument = new FlashDevelop.Mock.Docking.TabbedDocument();
            //tabbedDocument.Closing += new System.ComponentModel.CancelEventHandler(this.OnDocumentClosing);
            //tabbedDocument.Closed += new System.EventHandler(this.OnDocumentClosed);
            //tabbedDocument.TabPageContextMenuStrip = this.tabMenu;
            //tabbedDocument.ContextMenuStrip = this.editorMenu;
            tabbedDocument.Text = Path.GetFileName(file);
            tabbedDocument.AddEditorControls(file, text, codepage);
            //tabbedDocument.Show();
            return tabbedDocument;
        }

        /// <summary>
        /// Creates a floating panel for the plugin
        /// </summary>
        public DockContent CreateDockablePanel(Control ctrl, String guid, Image image, DockState defaultDockState)
        {
            DockablePanel dockablePanel = new DockablePanel(ctrl, guid);
            if (image != null) dockablePanel.Icon = ImageKonverter.ImageToIcon(image);
            dockablePanel.DockState = defaultDockState;
            LayoutManager.PluginPanels.Add(dockablePanel);
            return dockablePanel;
        }

        public bool CallCommand(string command, string arguments)
        {
            throw new NotImplementedException();
        }

        public List<ToolStripItem> FindMenuItems(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the specified menu item by name
        /// </summary>
        public ToolStripItem FindMenuItem(string name)
        {
            return StripBarManager.FindMenuItem(name);
        }

        public string ProcessArgString(string args)
        {
            throw new NotImplementedException();
        }

        public Keys GetShortcutItemKeys(string id)
        {
            throw new NotImplementedException();
        }

        public string GetThemeValue(string id)
        {
            throw new NotImplementedException();
        }

        public Color GetThemeColor(string id)
        {
            return Color.Empty;
        }

        public IPlugin FindPlugin(string guid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the specified composed/ready image
        /// </summary>
        public Image FindImage(string data)
        {
            return new Bitmap(16, 16);
        }

        /// <summary>
        /// Gets the Settings interface
        /// </summary>
        public ISettings Settings
        {
            get { return (ISettings)this.appSettings; }
        }

        /// <summary>
        /// Gets or sets the actual Settings
        /// </summary>
        public SettingObject AppSettings
        {
            get { return this.appSettings; }
            set { this.appSettings = value; }
        }

        /// <summary>
        /// Gets the tool strip
        /// </summary>
        public ToolStrip ToolStrip
        {
            get { return this.toolStrip; }
        }

        public MenuStrip MenuStrip
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the Scintilla configuration
        /// </summary>
        public Scintilla SciConfig
        {
            get { return ScintillaManager.SciConfig; }
        }

        /// <summary>
        /// Gets the DockPanel
        /// </summary> 
        public DockPanel DockPanel
        {
            get { return this.dockPanel; }
        }

        public string[] StartArguments
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the application custom args
        /// </summary>
        public List<Argument> CustomArguments
        {
            get { return ArgumentDialog.CustomArguments; }
        }

        public StatusStrip StatusStrip
        {
            get { throw new NotImplementedException(); }
        }

        public string WorkingDirectory
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ToolStripPanel ToolStripPanel
        {
            get { throw new NotImplementedException(); }
        }

        public ToolStripStatusLabel StatusLabel
        {
            get { throw new NotImplementedException(); }
        }

        public ToolStripStatusLabel ProgressLabel
        {
            get { throw new NotImplementedException(); }
        }

        public ToolStripProgressBar ProgressBar
        {
            get { throw new NotImplementedException(); }
        }

        public Control.ControlCollection Controls
        {
            get { return new Control.ControlCollection(this); }
        }

        public ContextMenuStrip TabMenu
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the EditorMenu
        /// </summary>
        public ContextMenuStrip EditorMenu
        {
            get { return this.editorMenu; }
        }

        /// <summary>
        /// Gets the CurrentDocument
        /// </summary>
        public ITabbedDocument CurrentDocument
        {
            get { return this.dockPanel.ActiveDocument as ITabbedDocument; }
        }

        public ITabbedDocument[] Documents
        {
            get
            {
                List<ITabbedDocument> documents = new List<ITabbedDocument>();
                foreach (DockPane pane in DockPanel.Panes)
                {
                    if (pane.DockState == DockState.Document)
                    {
                        foreach (IDockContent content in pane.Contents)
                        {
                            if (content is TabbedDocument)
                            {
                                documents.Add(content as TabbedDocument);
                            }
                        }
                    }
                }
                return documents.ToArray();
            }
        }

        public bool HasModifiedDocuments
        {
            get { throw new NotImplementedException(); }
        }

        public bool ClosingEntirely
        {
            get { throw new NotImplementedException(); }
        }

        public bool ProcessIsRunning
        {
            get { throw new NotImplementedException(); }
        }

        public bool ReloadingDocument
        {
            get { throw new NotImplementedException(); }
        }

        public bool ProcessingContents
        {
            get { throw new NotImplementedException(); }
        }

        public bool RestoringContents
        {
            get { throw new NotImplementedException(); }
        }

        public bool SavingMultiple
        {
            get { throw new NotImplementedException(); }
        }

        public bool PanelIsActive
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsFullScreen
        {
            get { return isFullScreen; }
        }

        public bool StandaloneMode
        {
            get { return false; }
        }

        public bool MultiInstanceMode
        {
            get { return false; }
        }

        public bool IsFirstInstance
        {
            get { return true; }
        }

        public bool RestartRequested
        {
            get { throw new NotImplementedException(); }
        }

        public bool RefreshConfig
        {
            get { throw new NotImplementedException(); }
        }

        public List<Keys> IgnoredKeys
        {
            get { return new List<Keys>(); }
        }

        public string ProductVersion
        {
            get { throw new NotImplementedException(); }
        }

        public string ProductName
        {
            get { throw new NotImplementedException(); }
        }

        public IntPtr Handle
        {
            get { return new IntPtr(); }
        }

        public bool PreFilterMessage(ref Message m)
        {
            throw new NotImplementedException();
        }

        public void New(Object sender, EventArgs e)
        {
            String fileName = DocumentManager.GetNewDocumentName(null);
            TextEvent te = new TextEvent(EventType.FileNew, fileName);
            EventManager.DispatchEvent(this, te);
            if (!te.Handled)
            {
                this.CreateEditableDocument(fileName, "", Encoding.UTF8.CodePage);
            }
        }

        public void ApplyAllSettings()
        {
            //if (this.InvokeRequired)
            //{
            //    this.BeginInvoke((MethodInvoker)delegate { this.ApplyAllSettings(); });
            //    return;
            //}
            //FlashDevelop.Managers.ShortcutManager.ApplyAllShortcuts();
            EventManager.DispatchEvent(this, new NotifyEvent(EventType.ApplySettings));
            for (int i = 0; i < this.Documents.Length; i++)
            {
                ITabbedDocument document = this.Documents[i];
                if (document.IsEditable)
                {
                    ScintillaManager.ApplySciSettings(document.SplitSci1, true);
                    ScintillaManager.ApplySciSettings(document.SplitSci2, true);
                }
            }
            //this.frInFilesDialog.UpdateSettings();
            //this.statusStrip.Visible = this.appSettings.ViewStatusBar;
            //this.toolStrip.Visible = this.isFullScreen ? false : this.appSettings.ViewToolBar;
            //ButtonManager.UpdateFlaggedButtons();
            //TabTextManager.UpdateTabTexts();
        }

    }
}