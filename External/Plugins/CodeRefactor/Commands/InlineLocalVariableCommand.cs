using System.Collections.Generic;
using System.Diagnostics;
using ASCompletion.Completion;
using ASCompletion.Model;
using CodeRefactor.Provider;
using PluginCore;
using PluginCore.Controls;
using PluginCore.FRService;
using PluginCore.Localization;
using PluginCore.Managers;

namespace CodeRefactor.Commands
{
    public class InlineLocalVariableCommand : RefactorCommand<IDictionary<string, List<SearchMatch>>>
    {
        readonly ASResult target;
        string value;
        readonly bool outputResults;
        readonly int startPos;
        FindAllReferences findAllReferencesCommand;

        /// <summary>
        /// A new Inline refactoring command.
        /// Outputs found results.
        /// Uses the current text location as the declaration target.
        /// </summary>
        public InlineLocalVariableCommand() : this(true)
        {
        }

        /// <summary>
        /// A new Inline refactoring command.
        /// Uses the current text location as the declaration target.
        /// </summary>
        /// <param name="outputResults">If true, will send the found results to the trace log and results panel</param>
        public InlineLocalVariableCommand(bool outputResults) : this(RefactoringHelper.GetDefaultRefactorTarget(), outputResults)
        {
        }

        /// <summary>
        /// A new Inline refactoring command.
        /// </summary>
        /// <param name="target">The target declaration to find references to.</param>
        /// <param name="outputResults">If true, will send the found results to the trace log and results panel</param>
        public InlineLocalVariableCommand(ASResult target, bool outputResults)
        {
            Debug.Assert(target != null, "target cannot be null.");
            Debug.Assert(target.Member != null, "target.Member cannot be null.");
            Debug.Assert((target.Member.Flags & FlagType.LocalVar) > 0, "target.Member should be local variable.");
            this.target = target;
            this.outputResults = outputResults;
            startPos = PluginBase.MainForm.CurrentDocument.SciControl.CurrentPos;
        }

        /// <summary>
        /// The concrete refactoring command execution implementation.
        /// This should be overridden by the derived classes with their custom refactoring logic.
        /// </summary>
        protected override void ExecutionImplementation()
        {
            var member = target.Member;
            value = member.Value;
            if (string.IsNullOrEmpty(value))
            {
                var sci = PluginBase.MainForm.CurrentDocument.SciControl;
                value = sci.GetLine(member.LineFrom);
                var index = value.IndexOfOrdinal("=");
                value = value.Substring(index + "=".Length);
                value = value.Trim('=', ' ', '\t', '\n', '\r', ';', '.');
            }
            findAllReferencesCommand = new FindAllReferences(target, false, false) {OnlySourceFiles = true};
            findAllReferencesCommand.OnRefactorComplete += OnFindAllReferencesCompleted;
            findAllReferencesCommand.Execute();
        }

        /// <summary>
        /// Indicates if the current settings for the refactoring are valid.
        /// </summary>
        public override bool IsValid()
        {
            return target != null && target.Member != null && (target.Member.Flags & FlagType.LocalVar) > 0;
        }

        void OnFindAllReferencesCompleted(object sender, RefactorCompleteEventArgs<IDictionary<string, List<SearchMatch>>> args)
        {
            UserInterfaceManager.ProgressDialog.Show();
            UserInterfaceManager.ProgressDialog.SetTitle(TextHelper.GetString("Info.UpdatingReferences"));
            MessageBar.Locked = true;
            var statusMessage = TextHelper.GetString("Info.Updating");
            foreach (var entry in args.Results)
            {
                var fileName = entry.Key;
                UserInterfaceManager.ProgressDialog.UpdateStatusMessage(statusMessage + " \"" + fileName + "\"");
                var doc = AssociatedDocumentHelper.LoadDocument(fileName);
                var sci = doc.SciControl;
                var matches = entry.Value;
                var member = target.Member;
                var lineFrom = member.LineFrom + 1;
                var match = matches.Find(it => it.Line == lineFrom);
                matches.Remove(match);
                var matchUnderCursor = matches.Find(it => it.Index <= startPos && it.Index + it.Length >= startPos);
                RefactoringHelper.ReplaceMatches(matches, sci, value);
                ASComplete.LocateMember("(var|const)", member.Name, member.LineFrom);
                var line = sci.CurrentLine;
                var lineLength = sci.LineLength(line);
                sci.LineDelete();
                doc.Save();
                if (matchUnderCursor.Line > line) matchUnderCursor.Index -= lineLength;
                var pos = matchUnderCursor.Index;
                sci.SetSel(pos, pos);
                break;
            }
            Results = args.Results;
            if (outputResults) ReportResults();
            UserInterfaceManager.ProgressDialog.Hide();
            MessageBar.Locked = false;
            FireOnRefactorComplete();
        }

        void ReportResults()
        {
            var newValueLength = value.Length;
            PluginBase.MainForm.CallCommand("PluginCommand", "ResultsPanel.ClearResults");
            foreach (var entry in Results)
            {
                var lineOffsets = new Dictionary<int, int>();
                var lineChanges = new Dictionary<int, string>();
                var reportableLines = new Dictionary<int, List<string>>();
                foreach (var match in entry.Value)
                {
                    var column = match.Column;
                    var lineNumber = match.Line;
                    var changedLine = lineChanges.ContainsKey(lineNumber) ? lineChanges[lineNumber] : match.LineText;
                    var offset = lineOffsets.ContainsKey(lineNumber) ? lineOffsets[lineNumber] : 0;
                    column = column + offset;
                    changedLine = changedLine.Substring(0, column) + value + changedLine.Substring(column + match.Length);
                    lineChanges[lineNumber] = changedLine;
                    lineOffsets[lineNumber] = offset + (newValueLength - match.Length);
                    if (!reportableLines.ContainsKey(lineNumber)) reportableLines[lineNumber] = new List<string>();
                    reportableLines[lineNumber].Add(entry.Key + ":" + match.Line + ": chars " + column + "-" + (column + newValueLength) + " : {0}");
                }
                foreach (var lineSetsToReport in reportableLines)
                {
                    var renamedLine = lineChanges[lineSetsToReport.Key].Trim();
                    foreach (var lineToReport in lineSetsToReport.Value)
                    {
                        TraceManager.Add(string.Format(lineToReport, renamedLine), (int)TraceType.Info);
                    }
                }
            }
            PluginBase.MainForm.CallCommand("PluginCommand", "ResultsPanel.ShowResults");
        }
    }
}