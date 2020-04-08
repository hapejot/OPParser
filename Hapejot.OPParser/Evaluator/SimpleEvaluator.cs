using System;
// using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace Hapejot.OPParser
{
    public class SimpleEvaluator
    {
        public object Calculate(string pSource, object env)
        {
            StringReader sr = new StringReader(pSource);
            TokenStream ts = new TokenStream(sr);
            ExpressionParser ep = new ExpressionParser(ts);
            ParseTreeNode n = ep.Parse();
            object r = Calculate(n, env);
            return r;
        }

        private object Calculate(ParseTreeNode n, object env)
        {
            object result = null;
            if (n != null)
            {
                if (n.Arity == 0)
                {
                    string r = null;
                    PropertyInfo prop = env.GetType().GetProperty(n.Name);
                    if (prop != null)
                    {
                        result = prop.GetValue(env, null);
                        if (result is string)
                        {
                            r = (string)result;
                        }
                    }
                    else
                    {
                        r = n.Name;
                    }
                    if (r != null)
                    {
                        double d0;
                        bool isDouble = double.TryParse(r, NumberStyles.AllowDecimalPoint, null, out d0);
                        if (isDouble)
                        {
                            result = d0;
                        }
                        else
                        {
                            DateTime dt;

                            bool isDate = DateTime.TryParse(r, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces, out dt);
                            if (isDate)
                            {
                                result = dt;
                            }
                        }
                    }
                }
                else if (n.Arity == 1)
                {
                    object a1 = Calculate(n[0], env);
                    MethodInfo meth = env.GetType().GetMethod(n.Name, new Type[] { a1.GetType() });
                    if (meth != null)
                    {
                        result = meth.Invoke(env, new object[] { a1 });
                    }
                    else
                    {
                        throw new Exception(String.Format("No Method named {0} with arguments ({1}).", n.Name, a1.GetType()));
                    }
                }
                else if (n.Arity == 2)
                {
                    object a1 = Calculate(n[0], env);
                    object a2 = Calculate(n[1], env);
                    MethodInfo meth = env.GetType().GetMethod(n.Name, new Type[] { a1.GetType(), a2.GetType() });
                    if (meth != null)
                    {
                        result = meth.Invoke(env, new object[] { a1, a2 });
                    }
                    else
                        throw new Exception(String.Format("No Method named {0} with arguments ({1}, {2}).", n.Name, a1.GetType(), a2.GetType()));
                }
                else
                {
                    result = String.Format("Unknown node type {0}", n.Name);
                }
            }
            return result;
        }
    }
}
