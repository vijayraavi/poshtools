using System;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PowerGuiVsx.Core.DebugEngine;

namespace PowerGUIVSX.Test
{
    [TestClass]
    public class ScriptDebuggerTest
    {
        private ScriptDebugger _debugger;
        private Runspace _runspace; 

        [TestInitialize]
        public void Init()
        {
            _runspace = RunspaceFactory.CreateRunspace();
            _runspace.Open();
            _runspace.SessionStateProxy.SetVariable("Test", "Test");

            _debugger = new ScriptDebugger(_runspace, new List<ScriptBreakpoint>());
        }

        [TestMethod]
        public void RefreshVariablesTest()
        {
            Assert.IsTrue(_debugger.Variables.ContainsKey("Test"));
            Assert.AreEqual("Test", _debugger.Variables["Test"].Value);
        }


    }
}
