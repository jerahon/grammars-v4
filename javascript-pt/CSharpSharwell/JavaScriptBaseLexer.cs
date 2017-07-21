using Antlr4.Runtime;
using System.Collections.Generic;
using static PT.PM.JavaScriptParseTreeUst.Parser.JavaScriptParser;

public abstract class JavaScriptBaseLexer : Lexer
{
    private Stack<bool> scopeStrictModes = new Stack<bool>();
    // The most recently produced token.
    private IToken _lastToken = null;
    private bool _useStrictDefault = false;
    private bool _useStrictCurrent = false;

    public JavaScriptBaseLexer(ICharStream input)
        : base(input)
    {
    }

    // A property indicating if the lexer should operate in strict mode.
    // When set to true, FutureReservedWords are tokenized, when false,
    // an octal literal can be tokenized.
    public bool UseStrictDefalult
    {
        get
        {
            return _useStrictDefault;
        }
        set
        {
            _useStrictDefault = value;
            _useStrictCurrent = value;
        }
    }

    public bool IsSrictMode()
    {
        return _useStrictCurrent;
    }

    ///<summary>Return the next token from the character stream and records this last
    ///token in case it resides on the default channel. This recorded token
    ///is used to determine when the lexer could possibly match a regex
    ///literal.</summary>
    ///<returns>the next token from the character stream.</returns>
    public override IToken NextToken()
    {
        // Get the next token.
        IToken next = base.NextToken();

        if (next.Type == OpenBrace)
        {
            _useStrictCurrent = scopeStrictModes.Count > 0 && scopeStrictModes.Peek() ? true : UseStrictDefalult;
            scopeStrictModes.Push(_useStrictCurrent);
        }
        else if (next.Type == CloseBrace)
        {
            _useStrictCurrent = scopeStrictModes.Count > 0 ? scopeStrictModes.Pop() : UseStrictDefalult;
        }
        else if (next.Type == StringLiteral &&
            (_lastToken == null || _lastToken.Type == OpenBrace) &&
            (next.Text.Substring(1, next.Text.Length - 2)) == "use strict")
        {
            if (scopeStrictModes.Count > 0)
                scopeStrictModes.Pop();
            _useStrictCurrent = true;
            scopeStrictModes.Push(_useStrictCurrent);
        }

        if (next.Channel == Lexer.DefaultTokenChannel)
        {
            // Keep track of the last token on the default channel.
            _lastToken = next;
        }

        return next;
    }

    ///<summary>Returns <c>true</c> iff the lexer can match a regex literal.</summary>
    ///<returns><c>true</c> iff the lexer can match a regex literal.</returns>
    protected bool RegexPossible()
    {
        if (_lastToken == null)
        {
            // No token has been produced yet: at the start of the input,
            // no division is possible, so a regex literal _is_ possible.
            return true;
        }

        switch (_lastToken.Type)
        {
            case Identifier:
            case NullLiteral:
            case BooleanLiteral:
            case This:
            case CloseBracket:
            case CloseParen:
            case OctalIntegerLiteral:
            case DecimalLiteral:
            case HexIntegerLiteral:
            case StringLiteral:
            case PlusPlus:
            case MinusMinus:
                // After any of the tokens above, no regex literal can follow.
                return false;
            default:
                // In all other cases, a regex literal _is_ possible.
                return true;
        }
    }
}