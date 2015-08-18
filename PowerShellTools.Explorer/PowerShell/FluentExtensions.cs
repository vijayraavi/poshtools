using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    internal static class FluentExtensions
    {
        internal static void AddPipelineSequence(this PowerShell ps, PipelineSequence sequence)
        {
            while (sequence.Count != 0)
            {
                PipelineCommand command = sequence.Next();
                ps.AddCommand(command.Command).AddParameters(command.Parameters);
            }
        }

        internal static PipelineCommand AddSwitchParameter(this PipelineCommand pc, string name)
        {
            return pc.AddParameter(name, true);
        }
    }
}