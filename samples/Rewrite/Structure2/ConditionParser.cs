﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rewrite.Structure2
{
    public class ConditionParser
    {
        private const char Percent = '%';
        private const char Money = '$';
        private const char Separator = ' ';
        private const char Colon = ':';
        private const char OpenBrace = '{';
        private const char CloseBrace = '}';
        public static List<ConditionsTestStringSegment> ParseCondition(string testString)
        {
            // TODO Create different parsers, regex, condition, etc.
            if (testString == null)
            {
                testString = String.Empty;
            }
            var context = new ConditionParserContext(testString);
            var results = new List<ConditionsTestStringSegment>();
            while (context.Next())
            {
                if (context.Current == Percent)
                {
                    // check for server request param
                    if (!context.Next())
                    {
                        throw new ArgumentException();
                    }
                    if (!ParseConditionParameter(context, results))
                    {
                        throw new ArgumentException();
                    }
                    // here store the information necessary to do an easy lookup
                }
                else if (context.Current == Money)
                {

                    context.Next();
                    context.Mark();
                    // variable
                    if (context.Current >= '0' && context.Current <= '9')
                    {
                        var ruleVariable = context.Capture();
                        results.Add(new ConditionsTestStringSegment { Type = StringCondtionType.RuleParameter, Variable = ruleVariable });
                    } else
                    {
                        throw new ArgumentException();
                    }
                }
                else
                {
                    if (!ParseLiteral(context, results))
                    {
                        throw new ArgumentException();
                    }
                }
            }
            return results;
        }

        // pre: will only be called if this is an unescaped '%' 
        // post: adds an application to the teststring results that will be concatinated on success
        private static bool ParseConditionParameter(ConditionParserContext context, List<ConditionsTestStringSegment> results)
        {
            if (context.Current == OpenBrace)
            {
                if (!context.Next())
                {
                    throw new ArgumentException();
                }
                context.Mark();
                while (context.Current != CloseBrace)
                {
                    if (!context.Next())
                    {
                        // reached end of string, its bad no matter what because no condition string
                        throw new ArgumentException();
                    }
                    else if (context.Current == Colon)
                    {
                        // Have a sub group in the server variable, do a lookup based on that.
                        // This means I need to return a dict form the lookup?
                    }
                    // TODO check if character is valid?
                }

                // capture.
                // TODO return result into list of operations.
                context.Back();
                var rawServerVariable = context.Capture();
                context.Next();
                if (IsValidVariable(context, rawServerVariable))
                {
                    results.Add(new ConditionsTestStringSegment { Type = StringCondtionType.ServerVariable, Variable = rawServerVariable });

                }
                else
                {
                    return false;
                }
            }
            else if (context.Current >= '0' && context.Current <= '9')
            {
                // means we have a segmented lookup
                // store information in the testString result to know what to look up.
                context.Mark();
                var rawConditionParameter = context.Capture();
                if (IsValidVariable(context, rawConditionParameter))
                {
                    results.Add(new ConditionsTestStringSegment { Type = StringCondtionType.ConditionParameter, Variable = rawConditionParameter });
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // illegal escape of a character
                throw new ArgumentException();
            }
            return true;
        }

        private static bool ParseLiteral(ConditionParserContext context, List<ConditionsTestStringSegment> results)
        {
            context.Mark();
            string encoded;
            while (true)
            {
                if (context.Current == Percent || context.Current == Money)
                {
                    context.Back();
                    encoded = context.Capture();
                    break;
                }
                if (!context.Next())
                {
                    context.Back();
                    encoded = context.Capture();
                    break;
                }
            }
            if (IsValidLiteral(context, encoded))
            {
                // add results
                results.Add(new ConditionsTestStringSegment { Type = StringCondtionType.Literal, Variable = encoded });
                return true;
            }
            else
            {
                return false;
            }
        }
        private static bool IsValidLiteral(ConditionParserContext context, string encoded)
        {
            // TODO
            return true;
        }
        private static bool IsValidVariable(ConditionParserContext context, string encoded)
        {
            return true;
        }

        private class ConditionParserContext
        {
            private readonly string _template;
            private int _index;
            private int? _mark;
            private HashSet<string> _parameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public ConditionParserContext(string condition)
            {
                _template = condition;
                _index = -1;
            }
            public char Current
            {
                get { return (_index < _template.Length && _index >= 0) ? _template[_index] : (char)0; }
            }
            public string Error
            {
                get;
                set;
            }
            public HashSet<string> ParameterNames
            {
                get { return _parameterNames; }
            }
            public bool Back()
            {
                return --_index >= 0;
            }
            public bool Next()
            {
                return ++_index < _template.Length;
            }
            public void Mark()
            {
                _mark = _index;
            }
            public string Capture()
            {
                if (_mark.HasValue)
                {
                    var value = _template.Substring(_mark.Value, _index - _mark.Value + 1);
                    _mark = null;
                    return value;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
