﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Math.Exceptions;

namespace Math
{
    public class Calculator
    {
        Operator[] operators = 
        {
            new Operator('^', 4, Operator.Associativity.Right, (args) => 
                {
                    return System.Math.Pow(args[0], args[1]);
                }),
            new Operator('*', 3, Operator.Associativity.Left, (args) =>
                {
                    return args[0] * args[1];
                }),
            new Operator('/', 3, Operator.Associativity.Left, (args) =>
                {
                    return args[0] / args[1];
                }),
            new Operator('+', 2, Operator.Associativity.Left, (args) =>
                {
                    return args[0] + args[1];
                }),
            new Operator('-', 2, Operator.Associativity.Left, (args) =>
                {
                    return args[0] - args[1];
                })
        };

        Function[] functions =
        {
            new Function("abs", 1, (args) =>
                {
                    return System.Math.Abs(args[0]);
                }),
            new Function("pi", 0, (args) =>
                {
                    return System.Math.PI;
                }),
                new Function("cos", 1, (args) =>
                {
                    return System.Math.Cos(args[0]);
                }),
                new Function("sin", 1, (args) =>
                {
                    return System.Math.Sin(args[0]);
                }),
                new Function("tan", 1, (args) =>
                {
                    return System.Math.Tan(args[0]);
                }),
                new Function("sqrt", 1, (args) =>
                {
                    return System.Math.Sqrt(args[0]);
                }),
                new Function("min", 2, (args) =>
                {
                    return System.Math.Min(args[0], args[1]);
                }),
                new Function("max", 2, (args) =>
                {
                    return System.Math.Max(args[0], args[1]);
                }),
                new Function("ceil", 1, (args) =>
                {
                    return System.Math.Ceiling(args[0]);
                }),
                new Function("floor", 1, (args) =>
                {
                    return System.Math.Floor(args[0]);
                }),
                new Function("round", 1, (args) =>
                {
                    return System.Math.Round(args[0]);
                })
        };

        Stack<Token> ops;
        Queue output;
        StringTokenizer tokenizer;

        // async solve?
        public double Solve(string expression)
        {
            ops = new Stack<Token>();
            output = new Queue();

            // Tokenize the input
            tokenizer = new StringTokenizer(expression, true);
            tokenizer.SymbolChars = new char[] { ',' };
            tokenizer.Operators = operators;
            tokenizer.Functions = functions;

            InfixToRPN();

            return EvaluateRPN();
        }

        private void InfixToRPN()
        {
            Token token = tokenizer.Next();

            while (token.Kind != TokenKind.EOF)
            {
                if (token.Kind == TokenKind.Number)
                {
                    //output.Enqueue(double.Parse((string)token.Value));
                    output.Enqueue(token);
                }
                else if (token.Kind == TokenKind.Function)
                {
                    ops.Push(token);
                }
                else if (token.Kind == TokenKind.Symbol)
                {
                    // function argument seperator
                    if (token.Value.ToString() == ",")
                    {
                        while (ops.Peek().Kind != TokenKind.LeftParentheses)
                        {
                            if (ops.Count == 0)
                            {
                                // separator misplaced or parentheses mismatched
                                throw new MismatchedParenthesisException();
                            }

                            output.Enqueue(ops.Pop());
                        }
                    }
                }
                else if (token.Kind == TokenKind.Operator)
                {
                    Operator op = (Operator)token.Value;
                    while (ops.Count > 0
                        && (
                            (op.assoc == Operator.Associativity.Left && op.precedence <= ((Operator)ops.Peek().Value).precedence)
                            ||
                            (op.assoc == Operator.Associativity.Right && op.precedence < ((Operator)ops.Peek().Value).precedence)
                        )
                    )
                    {
                        output.Enqueue(ops.Pop());
                    }

                    ops.Push(token);
                }
                else if (token.Kind == TokenKind.LeftParentheses)
                {
                    ops.Push(new Token(TokenKind.LeftParentheses, new Operator('(', 0, Operator.Associativity.None), 0, 0));
                    //ops.Push(new Operator('(', 0, Operator.Associativity.None));
                }
                else if (token.Kind == TokenKind.RightParentheses)
                {
                    Token op;
                    do
                    {
                        op = ops.Pop();
                        if (op.Kind != TokenKind.LeftParentheses)
                            output.Enqueue(op);


                        /* if (token.Kind == TokenKind.EOF && )
                         {
                             throw new MismatchedParenthesisException();
                         }*/
                    } while (op.Kind != TokenKind.LeftParentheses);
                }

                token = tokenizer.Next();
            }

            // since there are no more tokens, move onto the stack

            // while there are still operator tokens in the stack
            while (ops.Count > 0)
            {
                Operator nextOp = ((Operator)ops.Peek().Value);
                if (nextOp.symbol == '(' || nextOp.symbol == ')')
                {
                    throw new MismatchedParenthesisException();
                }
                else
                {
                    output.Enqueue(ops.Pop());
                }
            }
        }

        private double EvaluateRPN()
        {
            Stack<Token> evaluate = new Stack<Token>();
            do
            {
                Token next = (Token)output.Peek();
                if (next.Kind == TokenKind.Number)
                {
                    evaluate.Push((Token)output.Dequeue());
                }
                else
                {
                    // it's an operator (or function)
                    Operator op = (Operator)next.Value;

                    // if there are fewer than the number of arguments on the stack
                    if (evaluate.Count < op.numArgs)
                    {
                        // not enough values supplied
                        throw new Exception("not enough values supplied");
                    }
                    else
                    {
                        double[] args = new double[op.numArgs];

                        for (int i = op.numArgs - 1; i >= 0; i--)
                        {
                            Token t = ((Token)evaluate.Pop());
                            Console.WriteLine(t);
                            args[i] = double.Parse(t.Value.ToString());
                        }

                        output.Dequeue();

                        Token result = new Token(TokenKind.Number, op.PerformCalculation(args), 0, 0);
                        evaluate.Push(result);
                    }
                }
            } while (output.Count > 0);

            if (evaluate.Count == 1)
            {
                // this is the answer!
                return double.Parse(evaluate.Pop().Value.ToString());
            }
            else
            {
                // too many values were input
                throw new Exception("too many values supplied");
            }
        }
    }
}