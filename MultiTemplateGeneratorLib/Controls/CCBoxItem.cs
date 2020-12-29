namespace MultiTemplateGeneratorLib.Controls
{
    public class CCBoxItem
    {
        private int val;
        public int Value
        {
            get { return val; }
            set { val = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public CCBoxItem()
        {
        }

        public CCBoxItem(string name, int val)
        {
            this.name = name;
            this.val = val;
        }

        public override string ToString()
        {
            return string.Format("name: '{0}', value: {1}", name, val);
        }
    }
}
