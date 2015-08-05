using System.Collections.Generic;
using System.Linq;

namespace GetNugetDependendencies.DataStructures
{
    class TreeElement<T>
    {
        public T Element { get; private set; }
        private List<TreeElement<T>> Children;

        public TreeElement(T element)
        {
            this.Element = element;
            Children = new List<TreeElement<T>>();
        }

        public TreeElement<T> Get(T element)
        {
            if (this.Element.Equals(element)) return this;

            foreach(var e in Children)
            {
                if (e.Element.Equals(element)) return e;

                var sub = e.Get(element);
                if (sub != null) return sub;
            }

            return null;
        }

        public TreeElement<T> AddChild(T el)
        {
            var wrapped = new TreeElement<T>(el);
            this.Children.Add(wrapped);
            return wrapped;
        }

        public void AddChild(TreeElement<T> element)
        {
            this.Children.Add(element);
        }

        public bool Contains(T element)
        {
            if (this.Element.Equals(element)) return true;
            if (Children.Any(c => c.Contains(element))) return true;
            return false;
        }
    }
}
