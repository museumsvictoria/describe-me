using System;

namespace DescribeMe.Core.Infrastructure
{
    public class MultipleCommandHandlersFoundException : Exception
    {
        public MultipleCommandHandlersFoundException(Type type) : base(string.Format("Multiple Command handlers found for command type: {0}", type))
        {
        }
    }
}