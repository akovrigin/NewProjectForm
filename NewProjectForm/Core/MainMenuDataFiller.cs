using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewProjectForm.Core
{
    public class MainMenuDataFiller
    {
        private static TreeMenuData _recentMenuItem;
        private static int _recentCounterId = 10000;

        private static Collection<TreeMenuData> _treeMenuData; // = new Collection<TreeMenuData>();

        private static void RecursionFill(int id, TreeMenuData menuData, out TreeMenuData menuItem)
        {
            menuItem = null;

            var dt = DataBase.GetNodes();
            var rows = dt.Select($"{StringConst.FieldParentId} = {id}");

            if (!rows.Any())
                return;

            foreach (var dr in rows)
            {
                menuItem = new TreeMenuData(Convert.ToInt32(dr[StringConst.FieldId]), dr[StringConst.FieldName].ToString());
                menuData.MenuItems.Add(menuItem);

                if (dr[StringConst.FieldType].ToString().Equals(StringConst.Recent))
                    _recentMenuItem = menuItem;

                TreeMenuData newItem;

                RecursionFill(Convert.ToInt32(dr[StringConst.FieldId]), menuItem, out newItem);
            }
        }

        public static Collection<TreeMenuData> Fill()
        {
            _treeMenuData = new Collection<TreeMenuData>();

            var dt = DataBase.GetNodes();

            var rows = dt.Select($"{StringConst.FieldParentId} = {0}");

            foreach (DataRow dr in rows)
            {
                var menuItem = new TreeMenuData(Convert.ToInt32(dr[StringConst.FieldId]), dr[StringConst.FieldName].ToString());

                _treeMenuData.Add(menuItem);

                TreeMenuData newItem;
                RecursionFill(Convert.ToInt32(dr[StringConst.FieldId]), menuItem, out newItem);
            }

            return _treeMenuData;
        }

        public static void AddToRecent(string projectType)
        {
            DataBase.GetLeafs();

            //var menuItem = new TreeMenuData(_recentCounterId++, projectType);
            //_recentMenuItem.MenuItems.Add(menuItem);
        }
    }
}
