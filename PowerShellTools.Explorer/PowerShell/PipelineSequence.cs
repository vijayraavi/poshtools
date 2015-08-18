using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Explorer
{
    public class PipelineSequence
    {
        private Queue<PipelineCommand> _commands;

        public PipelineSequence()
        {
            _commands = new Queue<PipelineCommand>();
        }

        public int Count
        {
            get
            {
                return _commands.Count;
            }
        }

        public PipelineSequence Add(PipelineCommand command)
        {
            _commands.Enqueue(command);

            return this;
        }

        public PipelineCommand Next()
        {
            return _commands.Dequeue();
        }
    }
}
