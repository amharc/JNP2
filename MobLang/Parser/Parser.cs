using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MobLang.Exceptions;

namespace MobLang.Parser
{
    public class Parser
    {
        private static readonly char[][] OperatorFirstLetters =
        {
            new[] {';'},
            new[] {'$'},
            new[] {'@'},
            new[] {':'},
            new[] {'&', '|'},
            new[] {'=', '!', '<', '>'},
            new[] {'+', '-'},
            new[] {'*', '/', '%'},
            new[] {'^'}
        };

        private static readonly Associativity[] OperatorAssociativity =
        {
            Associativity.LeftAssociative,
            Associativity.RightAssociative,
            Associativity.LeftAssociative,
            Associativity.RightAssociative,
            Associativity.LeftAssociative,
            Associativity.NotAssociative,
            Associativity.LeftAssociative,
            Associativity.LeftAssociative,
            Associativity.RightAssociative
        };

        private readonly Tokenizer _tokenizer;
        private Expression _result;

        internal Parser(string source)
        {
            _tokenizer = new Tokenizer(source);
            _result = null;
        }

        internal Expression Result
        {
            get { return _result ?? (_result = ParseSource()); }
        }

        // ReSharper disable once UnusedParameter.Local
        private void Assert(bool condition, string error)
        {
            if (!condition)
                throw new ParserException(_tokenizer, error);
        }

        private void RequireToken(string token)
        {
            Assert(_tokenizer.Token != null, string.Format("{0} expected, but end-of-file found", token));
            Assert(_tokenizer.Advance() == token, string.Format("'{0}' expected but {1} found", token, _tokenizer.Token));
        }

        private Expression ParseSource()
        {
            var ex = ParseExpression();
            Assert(_tokenizer.Token == null, String.Format("Trailing tokens: {0}", _tokenizer.Token));
            return ex;
        }

        private Expression ParseExpression()
        {
            Assert(_tokenizer.Token != null, "Expression expected, but end-of-file found");

            switch (_tokenizer.Token)
            {
                case "def":
                    return ParseDefExpression();
                case "if":
                    return ParseIfExpression();
                case "match":
                    return ParseMatchExpression();
                case "fun":
                    return ParseLambdaExpression();
                case "data":
                    return ParseDataTypeDeclarationExpression();
                default:
                    return ParseSimpleExpression();
            }
        }

        private Expression ParseLambdaExpression()
        {
            RequireToken("fun");

            var patterns = new List<Pattern>();

            do
            {
                patterns.Add(ParsePattern());
            } while (_tokenizer.Token != "as");

            RequireToken("as");

            var body = ParseExpression();

            return new LambdaExpression(patterns, body);
        }

        private Expression ParseTupleExpression()
        {
            RequireToken("(");

            var list = new List<Expression>();

            if (_tokenizer.Token == ")")
            {
                _tokenizer.Advance();
                return new TupleExpression(list);
            }

            list.Add(ParseExpression());

            while (_tokenizer.Token == ",")
            {
                _tokenizer.Advance();
                list.Add(ParseExpression());
            }

            RequireToken(")");

            return list.Count == 1 ? list[0] : new TupleExpression(list);
        }

        private Expression ParseSimpleExpression(int level = 0)
        {
            if (level == OperatorFirstLetters.Length)
                return ParseApplication();
            var operands = new List<Expression>();
            var operators = new List<string>();

            operands.Add(ParseSimpleExpression(level + 1));

            while (_tokenizer.Token != null && OperatorFirstLetters[level].Contains(_tokenizer.Token[0]))
            {
                operators.Add(_tokenizer.Token);
                _tokenizer.Advance();
                operands.Add(ParseSimpleExpression(level + 1));
            }

            switch (OperatorAssociativity[level])
            {
                case Associativity.LeftAssociative:
                    return operands
                        .Skip(1)
                        .Zip(operators, (exp, op) => new {Exp = exp, Op = op})
                        .Aggregate(operands[0], (lhs, opRhs) =>
                            new ApplicationExpression(new ApplicationExpression(new VariableExpression(opRhs.Op), lhs),
                                opRhs.Exp)
                        );
                case Associativity.RightAssociative:
                    operands.Reverse();
                    operators.Reverse();
                    return operands
                        .Skip(1)
                        .Zip(operators, (exp, op) => new {Exp = exp, Op = op})
                        .Aggregate(operands[0], (rhs, opLhs) =>
                            new ApplicationExpression(
                                new ApplicationExpression(new VariableExpression(opLhs.Op), opLhs.Exp), rhs)
                        );
                default: //case Associativity.NOT_ASSOCIATIVE:
                    Assert(operands.Count <= 2, "This operator is not associative");
                    if (operands.Count == 1)
                        return operands[0];
                    return
                        new ApplicationExpression(
                            new ApplicationExpression(new VariableExpression(operators[0]), operands[0]), operands[1]);
            }
        }

        private static bool MayStartTerm(char x)
        {
            return char.IsLetterOrDigit(x) || x == '_' || x == '(' || x == '"' || x == '\'' || x == '[';
        }

        private Expression ParseApplication()
        {
            var res = ParseUnaryMinusExpression();

            while (_tokenizer.Token != null && !ReservedKeywords.Contains(_tokenizer.Token) &&
                   MayStartTerm(_tokenizer.Token[0]))
            {
                res = new ApplicationExpression(res, ParseTerm());
            }

            return res;
        }

        private string ParseName()
        {
            Assert(_tokenizer.Token != null, "Name expected, but end-of-file found");
            Assert(Char.IsLetter(_tokenizer.Token[0]) || _tokenizer.Token[0] == '_',
                String.Format("Letter expected, but {0} found", _tokenizer.Token[0]));
            Assert(_tokenizer.Token.Skip(1).All(x => Char.IsLetterOrDigit(x) || x == '_'), "Invalid character");
            Assert(!ReservedKeywords.Contains(_tokenizer.Token),
                String.Format("Non-keyword expected, but {0} found", _tokenizer.Token));
            return _tokenizer.Advance();
        }

        private Expression ParseUnaryMinusExpression()
        {
            if (_tokenizer.Token != "-")
                return ParseTerm();
            _tokenizer.Advance();

            var ex = ParseTerm();

            return new ApplicationExpression(new VariableExpression("negate"), ex);
        }

        private string ParseOperatorName()
        {
            RequireToken("operator");
            Assert(_tokenizer.Token.All(_tokenizer.IsSymbol),
                String.Format("Operator expected, but {0} found", _tokenizer.Token));
            Assert(OperatorFirstLetters.Any(x => x.Contains(_tokenizer.Token[0])),
                String.Format("Illegal first symbol: {0}", _tokenizer.Token[0]));
            return _tokenizer.Advance();
        }

        private Expression ParseTerm()
        {
            var term = ParseTermWithoutType();

            if (_tokenizer.Token != ":")
                return term;

            _tokenizer.Advance();
            return new TypeAnnotatedExpression(term, ParseTypeClause());
        }

        private Expression ParseTermWithoutType()
        {
            Assert(_tokenizer.Token != null, "Unexpected end of source");
            if (_tokenizer.Token == "operator")
                return new VariableExpression(ParseOperatorName());
            switch (_tokenizer.Token[0])
            {
                case '"':
                    return ParseStringLiteral();
                case '\'':
                    return ParseCharacterLiteral();
                case '[':
                    return ParseListLiteral();
                case '(':
                    return ParseTupleExpression();
            }
            if (_tokenizer.Token.All(Char.IsDigit))
                return new IntegralExpression(ParseIntegral());
            if (Char.IsLetter(_tokenizer.Token[0]) || _tokenizer.Token[0] == '_')
                return new VariableExpression(ParseName());
          
            throw new ParserException(_tokenizer, String.Format("Unexpected token: {0}", _tokenizer.Token));
        }

        private Expression ParseListLiteral()
        {
            RequireToken("[");
            var elements = new List<Expression>();

            if (_tokenizer.Token != "]")
            {
                while (true)
                {
                    elements.Add(ParseExpression());
                    if (_tokenizer.Token == "]")
                        break;
                    RequireToken(",");
                }
            }

            RequireToken("]");

            return elements.Reverse<Expression>().Aggregate<Expression, Expression>(
                ExpressionDefaults.Nil,
                (rest, self) => new ApplicationExpression(new ApplicationExpression(ExpressionDefaults.Cons, self), rest)
                );
        }

        private Expression ParseCharacterLiteral()
        {
            Assert(_tokenizer.Token != null && _tokenizer.Token[0] == '\'' && _tokenizer.Token.Length == 2,
                String.Format("Character literal expected, but {0} found", _tokenizer.Token));
            return new CharacterExpression(_tokenizer.Advance()[1]);
        }

        private Expression ParseStringLiteral()
        {
            Assert(_tokenizer.Token != null && _tokenizer.Token[0] == '"',
                String.Format("String literal expected, but {0} found", _tokenizer.Token));
            return _tokenizer.Advance().Skip(1).Reverse().Aggregate<char, Expression>(
                ExpressionDefaults.Nil,
                (rest, self) =>
                    new ApplicationExpression(
                        new ApplicationExpression(ExpressionDefaults.Cons, new CharacterExpression(self)), rest)
                );
        }

        private DefClause ParseDefClause()
        {
            string name;

            if (_tokenizer.Token == "operator")
            {
                name = ParseOperatorName();
            }
            else
            {
                name = ParseName();
                Assert(!Char.IsUpper(name[0]), "Binding name should not start with an uppercase letter");
            }

            var arguments = new List<Pattern>();

            while (_tokenizer.Token != null && _tokenizer.Token != "as" && _tokenizer.Token != ":" && _tokenizer.Token != ")")
                arguments.Add(ParsePattern());

            TypeClause type = null;

            if (_tokenizer.Token == ":")
            {
                _tokenizer.Advance();
                type = ParseTypeClause();
            }

            RequireToken("as");

            var body = ParseExpression();

            return type != null ? new TypeAnnotatedDefFunctionClause(name, arguments, body, type) : new DefFunctionClause(name, arguments, body);
        }

        private Expression ParseDefExpression()
        {
            RequireToken("def");

            var clauses = new List<DefClause> {ParseDefClause()};

            while (_tokenizer.Token == "and")
            {
                _tokenizer.Advance();
                clauses.Add(ParseDefClause());
            }

            if (_tokenizer.Token != "in")
                return new DefExpression(clauses);

            _tokenizer.Advance();
            return new DefInExpression(clauses, ParseExpression());
        }


        private Pattern ParsePattern()
        {
            if (_tokenizer.Token == "_")
                return ParseWildcardPattern();
            if (_tokenizer.Token.All(Char.IsDigit))
                return ParseIntegralPattern();
            if (_tokenizer.Token == "(")
            {
                RequireToken("(");
                var res = ParseBracketedPattern();
                RequireToken(")");
                return res;
            }
            return ParseBracketedPatternWithoutType();
        }

        private Pattern ParseIntegralPattern()
        {
            return new IntegralPattern(ParseIntegral());
        }

        private BigInteger ParseIntegral()
        {
            BigInteger integral;
            Assert(BigInteger.TryParse(_tokenizer.Advance(), out integral), "Illegal number");
            return integral;
        }

        private Pattern ParseWildcardPattern()
        {
            RequireToken("_");
            return new WildcardPattern();
        }

        private Pattern ParseBracketedPattern()
        {
            var pattern = ParseBracketedPatternWithoutType();

            if (_tokenizer.Token != ":")
                return pattern;

            _tokenizer.Advance();
            var type = ParseTypeClause();
            return new TypeAnnotatedPattern(pattern, type);
        }

        private Pattern ParseBracketedPatternWithoutType()
        {
            var list = new List<Pattern>();

            Assert(!ReservedKeywords.Contains(_tokenizer.Token), "Unexpected token: " + _tokenizer.Token);

            if (_tokenizer.Token == ")" || _tokenizer.Token == ":")
                return new TuplePattern(list);

            list.Add(ParseNestedPattern());

            while (_tokenizer.Token == ",")
            {
                _tokenizer.Advance();
                list.Add(ParsePattern());
            }

            return list.Count == 1 ? list[0] : new TuplePattern(list);
        }

        private Pattern ParseNestedPattern()
        {
            if (_tokenizer.Token == "(")
            {
                RequireToken("(");
                var res = ParseBracketedPattern();
                RequireToken(")");
                return res;
            }

            var name = ParseName();
            if (!Char.IsUpper(name[0]))
                return new NamePattern(name);
            var arguments = new List<Pattern>();

            while (_tokenizer.Token != "," && _tokenizer.Token != ")" && _tokenizer.Token != "then" && _tokenizer.Token != "if" && _tokenizer.Token != ":")
            {
                arguments.Add(ParsePattern());
            }

            return new DataPattern(name, arguments);
        }

        private Expression ParseIfExpression()
        {
            RequireToken("if");

            var condition = ParseSimpleExpression();

            RequireToken("then");

            var ifTrue = ParseExpression();

            if (_tokenizer.Token != "else")
                return new IfExpression(condition, ifTrue);

            _tokenizer.Advance();
            var ifFalse = ParseExpression();

            return new IfExpression(condition, ifTrue, ifFalse);
        }

        private Expression ParseMatchExpression()
        {
            RequireToken("match");

            var value = ParseSimpleExpression();

            var cases = new List<MatchCase>();

            do
            {
                RequireToken("with");

                var pattern = ParsePattern();
                Expression condition = null;

                if (_tokenizer.Token == "if")
                {
                    _tokenizer.Advance();

                    condition = ParseSimpleExpression();
                }

                RequireToken("then");

                var body = ParseExpression();

                cases.Add(condition == null
                    ? new MatchCase(pattern, body)
                    : new ConditionalMatchCase(pattern, condition, body));
            } while (_tokenizer.Token == "with");

            return new MatchExpression(value, cases);
        }

        private Expression ParseDataTypeDeclarationExpression()
        {
            RequireToken("data");

            var name = ParseName();
            Assert(Char.IsUpper(name[0]), "Type names should start with an uppercase letter");

            var arguments = new List<string>();

            while (_tokenizer.Token != null && !ReservedKeywords.Contains(_tokenizer.Token))
            {
                var arg = ParseName();
                Assert(Char.IsLower(arg[0]), "Type variable names should start with an lowercase letter");
                arguments.Add(arg);
            }

            var constructors = new List<DataTypeConstructorDeclaration>();

            while (_tokenizer.Token == "case")
            {
                constructors.Add(ParseDataTypeConstructorDeclaration());
            }

            return new DataTypeDeclarationExpression(name, arguments, constructors);
        }

        private DataTypeConstructorDeclaration ParseDataTypeConstructorDeclaration()
        {
            RequireToken("case");

            var name = ParseName();
            Assert(Char.IsUpper(name[0]), "Constructor names should start with an uppercase letter");

            if (_tokenizer.Token != "of")
                return new DataTypeConstructorDeclaration(name, new List<TypeClause>());

            RequireToken("of");

            var arguments = new List<TypeClause>();

            while (_tokenizer.Token != null && !ReservedKeywords.Contains(_tokenizer.Token))
                arguments.Add(ParseTypeClause());

            return new DataTypeConstructorDeclaration(name, arguments);
        }

        private TypeClause ParseTypeClause()
        {
            return ParseFunctionTypeClause();
        }

        private TypeClause ParseSingleTypeClause()
        {
            if (_tokenizer.Token == "(")
            {
                _tokenizer.Advance();
                var clause = ParseTupleTypeClause();
                RequireToken(")");
                return clause;
            }
            var name = ParseName();

            Assert(Char.IsLetter(name[0]), "Letter expected");

            if (Char.IsLower(name[0]))
                return new VariableTypeClause(name);
            var list = new List<TypeClause>();

            while (_tokenizer.Token != null && !ReservedKeywords.Contains(_tokenizer.Token) && _tokenizer.Token != ")" &&
                   _tokenizer.Token != "," && _tokenizer.Token != "->")
                list.Add(ParseSimpleTypeClause());

            return new NamedTypeClause(name, list);
        }

        private TypeClause ParseTupleTypeClause()
        {
            var list = new List<TypeClause> {ParseTypeClause()};

            while (_tokenizer.Token == ",")
            {
                _tokenizer.Advance();
                list.Add(ParseTypeClause());
            }

            return list.Count > 1 ? new TupleTypeClause(list) : list[0];
        }

        private TypeClause ParseFunctionTypeClause()
        {
            var arg = ParseSingleTypeClause();
            if (_tokenizer.Token != "->")
                return arg;
            _tokenizer.Advance();
            return new FunctionTypeClause(arg, ParseFunctionTypeClause());
        }

        private TypeClause ParseSimpleTypeClause()
        {
            if (_tokenizer.Token == "(")
                return ParseSingleTypeClause();

            var name = ParseName();

            if (char.IsLower(name[0]))
                return new VariableTypeClause(name);
            return new NamedTypeClause(name, new List<TypeClause>());
        }

        private enum Associativity
        {
            LeftAssociative,
            RightAssociative,
            NotAssociative
        };

        public static readonly string[] ReservedKeywords =
        {
            "def", "as", "in", "if", "fun", "then", "else", "match", "with", "operator", "data", "case", "of", "and"
        };
    }
}