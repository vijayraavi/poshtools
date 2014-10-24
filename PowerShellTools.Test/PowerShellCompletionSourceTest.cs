using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Moq;
using PowerShellTools.Intellisense;

namespace PowerShellTools.Test
{
    [TestClass]
    public class PowerShellCompletionSourceTest
    {
        private PowerShellCompletionSource _source;
        private Runspace _runspace;
        private Mock<IGlyphService> _glyphService;
        private Mock<ICompletionSession> _completionSession;
        private Mock<ITextView> _textView;
        private Mock<ITextBuffer> _textBuffer;
        private Mock<ITextSnapshot> _textSnapshot;
        private Mock<ITextCaret> _textCaret;

        [TestInitialize]
        public void Init()
        {
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
            
            _glyphService = new Mock<IGlyphService>();
            _completionSession = new Mock<ICompletionSession>();
            _textView = new Mock<ITextView>();
            _textBuffer = new Mock<ITextBuffer>();
            _textSnapshot = new Mock<ITextSnapshot>();
            _textCaret = new Mock<ITextCaret>();

            _completionSession.Setup(m => m.TextView).Returns(_textView.Object);
            _textView.Setup(m => m.TextBuffer).Returns(_textBuffer.Object);
            _textView.Setup(m => m.Caret).Returns(_textCaret.Object);
            _textBuffer.Setup(m => m.CurrentSnapshot).Returns(_textSnapshot.Object);

            _source = new PowerShellCompletionSource(_runspace, _glyphService.Object);   
        }

        public void Cleanup()
        {
            _runspace.Dispose();
        }

        [TestMethod]
        public void ShouldCompleteParameters()
        {
            _textSnapshot.Setup(m => m.GetText()).Returns("Get-Process -");

            var position = new CaretPosition();
            position.BufferPosition.Add(12);

            _textCaret.Setup(m => m.Position).Returns(position);

            var completionSet = new List<CompletionSet>();
            _source.AugmentCompletionSession(_completionSession.Object, completionSet);

            Assert.AreEqual("Xyz", completionSet.First().Completions.First().InsertionText);
        }
    }
}
