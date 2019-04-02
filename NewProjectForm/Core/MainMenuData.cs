using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewProjectForm.Core
{
    public class TreeMenuData
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public ObservableCollection<TreeMenuData> MenuItems { get; private set; }

        public TreeMenuData(int id, string name)
        {
            Id = id;
            Name = name;
            MenuItems = new ObservableCollection<TreeMenuData>();
        }
    }
}
