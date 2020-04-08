namespace Hapejot.OPParser
{
    public class ValueNode
    {
        private string _name;
        private object[] _args;

        public ValueNode(string name, params object[] args)
        {
            _name = name;
            _args = args;
        }

        public string Name { get => _name;  }
        public object[] Arg { get => _args; }
    }
}