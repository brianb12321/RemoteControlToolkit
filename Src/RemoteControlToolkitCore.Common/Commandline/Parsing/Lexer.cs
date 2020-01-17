using System.Collections.Generic;
using System.Text;

namespace RemoteControlToolkitCore.Common.Commandline.Parsing
{
    public class Lexer : ILexer
    {
        private void lexWord(List<CommandToken> tokens, ref int index, string text, TokenType type)
        {
            StringBuilder sb = new StringBuilder(text[index].ToString());
            for (int j = index + 1; j < text.Length; ++j)
            {
                if (text[j] == ' ')
                {
                    tokens.Add(new CommandToken(sb.ToString(), type));
                    break;
                }
                else
                {
                    sb.Append(text[j]);
                    ++index;
                }
            }

            if (index == text.Length - 1)
            {
                tokens.Add(new CommandToken(sb.ToString(), type));
            }
        }

        private void lexBoundary(List<CommandToken> tokens, ref int index, string Text, char end, TokenType type)
        {
            StringBuilder sb = new StringBuilder();
            bool escaped = false;
            bool foundEndingSymbol = false;
            for (int j = index + 1; j < Text.Length; ++j)
            {
                if (Text[j] == '\\')
                {
                    if (escaped)
                    {
                        escaped = false;
                        sb.Append(Text[j]);
                        ++index;
                    }
                    else
                    {
                        escaped = true;
                        index++;
                    }
                }
                else if (Text[j] == end && escaped != true)
                {
                    foundEndingSymbol = true;
                    tokens.Add(new CommandToken(sb.ToString(), type));
                    ++index;
                    break;
                }
                else
                {
                    sb.Append(Text[j]);
                    if (escaped) escaped = false;
                    ++index;
                }
            }

            if (!foundEndingSymbol)
            {
                throw new ParserException($"Expected {end} found EOL.");
            }
        }

        public IReadOnlyList<CommandToken> Lex(string input)
        {
            List<CommandToken> _tokens = new List<CommandToken>();
            for (int i = 0; i < input.Length; i++)
            {
                char currentChar = input[i];
                switch (currentChar)
                {
                    case ' ':
                        break;
                    case '"':
                        lexBoundary(_tokens, ref i, input, '"', TokenType.Quote);
                        break;
                    case '{':
                        lexBoundary(_tokens, ref i, input, '}', TokenType.Script);
                        break;
                    case '>':
                        int newI;
                        if (input.Length <= i + 1)
                        {
                            throw new ParserException("EOF while scanning for redirected file or resource.");
                        }

                        if (input[i + 1] == '>')
                        {
                            newI = i + 3;
                            lexWord(_tokens, ref newI, input, TokenType.AppendOutRedirect);
                        }
                        else
                        {
                            newI = i + 2;
                            lexWord(_tokens, ref newI, input, TokenType.OutRedirect);
                        }

                        break;
                    case '<':
                        if (input.Length <= i + 1)
                        {
                            throw new ParserException("EOF while scanning for redirected file or resource.");
                        }

                        int newI2 = i + 2;
                        lexWord(_tokens, ref newI2, input, TokenType.InRedirect);
                        break;
                    case '$':
                        lexWord(_tokens, ref i, input, TokenType.EnvironmentVariable);
                        break;
                    case '|':
                        _tokens.Add(new CommandToken("|", TokenType.Pipe));
                        break;
                    default:
                        lexWord(_tokens, ref i, input, TokenType.Word);
                        break;
                }
            }

            return _tokens;
        }
    }
}