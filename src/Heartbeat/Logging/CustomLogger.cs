using Microsoft.Extensions.Logging;

namespace Heartbeat.Hosting.Console.Logging
{
    public sealed class CustomLogger : ILogger
    {
        private State _currentState = new(0);
        private readonly Stack<State> _stateStack = new();
        private readonly TextWriter _textWriter = System.Console.Out;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            ConsoleColor? savedForegroundColor = null;

            if (logLevel == LogLevel.Warning)
            {
                savedForegroundColor = System.Console.ForegroundColor;
                System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            }

            _textWriter.Write(_currentState.IndentionString);
            _textWriter.WriteLine(state?.ToString());

            if (savedForegroundColor != null)
            {
                System.Console.ForegroundColor = savedForegroundColor.Value;
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            _textWriter.Write(_currentState.IndentionString);
            _textWriter.Write("|> ");
            _textWriter.Write(state?.ToString());
            _textWriter.WriteLine(':');

            _stateStack.Push(_currentState);
            _currentState = new State(_currentState.IndentionLevel + 1);

            return new CustomScope(this);
        }

        private void PopState()
        {
            _currentState = _stateStack.Pop();
        }

        private class State
        {
            public int IndentionLevel { get; }
            public string IndentionString { get; }

            public State(int indentionLevel)
            {
                if (indentionLevel < 0)
                {
                    throw new(nameof(indentionLevel));
                }

                IndentionLevel = indentionLevel;

                IndentionString = string.Empty;
                for (var i = 0; i < indentionLevel; i++)
                {
                    IndentionString += "    ";
                }
            }
        }

        private class CustomScope : IDisposable
        {
            private readonly CustomLogger _logger;

            public CustomScope(CustomLogger logger)
            {
                _logger = logger;
            }

            public void Dispose()
            {
                //_logger.LogInformation(string.Empty); // TODO write only if there was a log after latest dispose
                _logger.PopState();
            }
        }
    }
}