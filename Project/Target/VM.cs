using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Target
{
    public class VM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string _memo;
        public string Memo
        {
            get { return _memo; }
            set
            {
                _memo = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Memo"));
            }
        }

        public class CommandOKCore : ICommand
        {
            public event EventHandler CanExecuteChanged;
            public bool CanExecute(object parameter) { return true; }
            public void Execute(object parameter) { }
        }

        public ICommand CommandOK { get { return new CommandOKCore(); } }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
        public IEnumerable<Person> Persons { get; private set; }

        public VM()
        {
            Persons = Enumerable.Range('A', 26).Select(e => new Person() { Age = 30, Name = ((char)e).ToString() });
        }
    }
}
