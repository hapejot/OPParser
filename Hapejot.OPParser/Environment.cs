using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Reflection;

namespace Hapejot.OPParser
{
    public class Environment
    {
        public delegate object TreeFunction(ParseTreeNode node);

        private class StackFrame
        {

            public StackFrame Parent { get; set; }
            public Hashtable Values { get; set; }


            public object this[string name]
            {
                get
                {
                    object result = null;
                    if (Values.ContainsKey(name))
                        result = Values[name];
                    else if (Parent != null)
                    {
                        result = Parent[name];
                    }
                    else
                        throw new KeyNotFoundException(name);
                    return result;
                }
                set
                {
                    Values[name] = value;
                }
            }
            public StackFrame()
            {
                Parent = null;
                Values = new Hashtable();
            }
            public StackFrame(StackFrame parent)
            {
                Parent = parent;
                Values = new Hashtable();
            }
        }
        private StackFrame _stackframe = new StackFrame();
        private Dictionary<string, TreeFunction> _treefun = new Dictionary<string, TreeFunction>();
        public void Register(string name, TreeFunction fun)
        {
            _treefun[name] = fun;
        }
        public object this[string name]
        {
            get { return _stackframe[name]; }
            set { _stackframe[name] = value; }
        }
        public void Enter()
        {
            _stackframe = new StackFrame(_stackframe);
        }
        public void Exit()
        {
            _stackframe = _stackframe.Parent;
        }
        public Environment()
        {
            Register("APPLY", new TreeFunction(Apply));
            Register("STRING", new TreeFunction(ReturnString));
            Register("NUMBER", new TreeFunction(ReturnNumber));
            Register("PAREN", new TreeFunction(ReturnExpr));
        }


        public object Evaluate(ParseTreeNode x)
        {
            object result = null;
            MethodTracer.Enter();

            if (_treefun.ContainsKey(x.Name))
            {
                MethodTracer.LogMessage("FUN:{0}", x.Name);
                result = _treefun[x.Name](x);
            }
            else
            {
                try
                {
                    result = this[x.Name];
                }
                catch (KeyNotFoundException)
                {
                    result = null;
                }
            }
            TraceExpression(x, result);
            MethodTracer.Exit();
            return result;
        }

        private static object ReturnString(ParseTreeNode x)
        {
            object result = x[0].Name;
            return result;
        }
        private static object ReturnNumber(ParseTreeNode x)
        {
            decimal result = decimal.Parse(x[0].Name);
            return result;
        }
        private object ReturnExpr(ParseTreeNode x)
        {
            object result = Evaluate(x[0]);
            return result;
        }

        private object Apply(ParseTreeNode x)
        {
            object result = null;
            object o = Evaluate(x[0]);
            if (o is MethodInfo)
            {
                MethodInfo meth = (MethodInfo)o;
                object[] paras = new object[x.Arity - 1];
                for (int i = 1; i < x.Arity; i++) paras[i - 1] = x[i];
                try
                {
                    result = meth.Invoke(this, paras);
                }
                catch (TargetInvocationException ex1)
                {
                    MethodTracer.LogWarning(ex1.Message);
                    MethodTracer.LogWarning(ex1.InnerException.Message);
                    MethodTracer.LogWarning(ex1.InnerException.StackTrace);
                }
            }
            return result;
        }

        private static void TraceExpression(ParseTreeNode x, object result)
        {
            StringBuilder sb = new StringBuilder();
            x.PrintTo(sb);
            MethodTracer.LogMessage("EVAL:{0} -> {1}", sb, result);
        }
    }
}
