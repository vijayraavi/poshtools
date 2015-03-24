using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Moq;
using PowerShellTools.Intellisense;

namespace PowerShellTools.Test.IntelliSense
{
    [TestClass]
    public class AutoCompletionControllerUnitTests
    {
        private string _mockedScript;
        private int _mockedCaret;

        [TestMethod]
        public void TestCompleteBrace()
        {
            TestCompleteBraceOrQuotesHelper('(', @"MyFunc", 6, @"MyFunc()", 7);
        }

        [TestMethod]
        public void TestCompleteQuotes()
        {
            TestCompleteBraceOrQuotesHelper('\"', @"MyFunc()", 7, @"MyFunc("""")", 8);
        }

        private void TestCompleteBraceOrQuotesHelper(char leftBraceOrQuotes, string initialScript, int initialCaret, string expectedScript, int expectedCaret)
        {
            Mock<ITextView> textView =  new Mock<ITextView>();
            var editorOperations = EditorOperationsMock();
            var textUndoHistory = TextUndoHistoryMock();
            Mock<SVsServiceProvider> serviceProvider = new Mock<SVsServiceProvider>();
            AutoCompletionController autoCompletionController = new AutoCompletionController(textView.Object,
                                                                                              editorOperations.Object,
                                                                                              textUndoHistory.Object,
                                                                                              serviceProvider.Object);
            _mockedScript = initialScript;
            _mockedCaret = initialCaret;
            autoCompletionController.CompleteBraceOrQuotes(leftBraceOrQuotes);
            Assert.AreEqual<int>(expectedCaret, _mockedCaret);
            Assert.AreEqual<string>(expectedScript, _mockedScript);
        }

        private void TextViewMockHelper(Mock<ITextView> textView)
        {

        }

        private Mock<IEditorOperations> EditorOperationsMock()
        {
            Mock<Microsoft.VisualStudio.Text.Operations.IEditorOperations> editorOperations = new Mock<IEditorOperations>();
            editorOperations.Setup(m => m.AddBeforeTextBufferChangePrimitive()).Callback(() => { });
            editorOperations.Setup(m => m.AddAfterTextBufferChangePrimitive()).Callback(() => { });
            editorOperations.Setup(m => m.InsertText(It.IsAny<string>()))
                            .Callback<string>(p =>
                                                {
                                                    _mockedScript = _mockedScript.Insert(_mockedCaret, p);
                                                    _mockedCaret++;
                                                });
            editorOperations.Setup(m => m.MoveToPreviousCharacter(false))
                            .Callback(() =>
                                        {
                                            _mockedCaret--;
                                        });
            return editorOperations;
        }

        private Mock<ITextUndoHistory> TextUndoHistoryMock()
        {
            Mock<ITextUndoHistory> textUndoHistory = new Mock<ITextUndoHistory>();
            Mock<ITextUndoTransaction> textUndoTransactioin = new Mock<ITextUndoTransaction>();
            textUndoTransactioin.Setup(m => m.Complete()).Callback(() => {});
            textUndoHistory.Setup(m => m.CreateTransaction(It.IsAny<string>())).Returns(() => textUndoTransactioin.Object);
            return textUndoHistory;
        }
    }
}
