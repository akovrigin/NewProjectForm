using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewProjectForm.Core
{
    public class DataBase
    {
        private const string DataStorageDataSet = "DataStorage";
        private const string NodeTableName = "NodeTable";
        private const string LeafTableName = "LeafTable";

        private static int _recentCounterId = 10000;
        private static int _recentId = 10;

        private static readonly DataSet DataStorage;

        #region Data

        private static readonly object[,] NodeDbTable =
        {
            {1, 0, StringConst.Recent, StringConst.Recent, 1},

            {_recentId, 1, StringConst.Recent, "All", 1},

            {2, 0, StringConst.Installed, StringConst.Installed, 1},
            {210, 2, StringConst.Installed, "Templates", 1},
            {211, 210, StringConst.Installed, "Bisness Intelligence", 1},
            {220, 211, StringConst.Installed, "Analysis Services", 1},

            {230, 211, StringConst.Installed, "Integration Services", 1},

            {240, 211, StringConst.Installed, "Reporting Services", 1},

            {250, 210, StringConst.Installed, "Visual C#", 1},
            {260, 250, StringConst.Installed, "Windows", 1},
            {263, 260, StringConst.Installed, "Universal", 1},
            {265, 260, StringConst.Installed, "Windows 8", 1},
            {267, 260, StringConst.Installed, "Classic Desktop", 1},

            {270, 260, StringConst.Installed, "Web", 1},

            {3, 0, StringConst.Online, StringConst.Online, 1},
            {310, 3, StringConst.Online, "Templates", 1},
            {350, 3, StringConst.Online, "Samples", 1},
        };

        private static readonly object[,] LeafDbTable =
        {
            {101, 110, "Recently opened project type 1", 1, ""},
            {102, 110, "Recently opened project type 2", 1, ""},

            {221, 220, "Import from Server", 1, "2.0;3.0;4.0"},
            {222, 220, "Analysis Services Tabular Project", 1, "3.0;4.0"},
            {223, 220, "Import from PowerPivot", 1, "4.0"},

            {231, 230, "Integration Services Import Project Wizard", 1, "2.0;3.0;4.0"},

            {241, 240, "Reporting Services Project", 1, "2.0;3.0"},
            {242, 240, "Reporting Services Import", 1, "3.0"},

            {251, 263, "Universal Windows Tools", 1, "2.0;3.0;4.0;4.5"},
            {252, 265, "Class Library (Portable for iOS, Android and Windows)", 1, "4.0;4.5"},
            {253, 265, "Windows 8.1 and Windows Phone 8.0/8.1 Tools", 1, "4.0;4.5"},
            {254, 267, "Windows Forms Application", 1, "2.0;3.0;4.0;4.5"},
            {255, 267, "WPF Application", 1, "3.0;4.0;4.5"},
            {256, 267, "Console Application", 1, "2.0;3.0;4.0;4.5"},
            {257, 267, "Class Library", 1, "2.0;3.0;4.0;4.5"},
            {258, 267, "Empty Project", 1, "2.0;3.0;4.0;4.5"},

            {261, 270, "ASP.NET Web Application (.NET Framework)", 1, "2.0;3.0;4.0;4.5"},
            {262, 270, "ASP.NET Core Web Application (.NET Core)", 1, "4.5"},
            {263, 270, "ASP.NET Core Application (.NET Framework)", 1, "2.0;3.0;4.0;4.5"},
        };

        #endregion Data

        static DataBase()
        {
            DataStorage = new DataSet(DataStorageDataSet);

            CreateDataSetStructure();

            LoadDataFromDb();
        }

        public static DataTable GetNodes()
        {
            return DataStorage.Tables[NodeTableName];
        }

        public static DataTable GetLeafs()
        {
            return DataStorage.Tables[LeafTableName];
        }

        private static void CreateDataSetStructure()
        {
            var table = new DataTable(NodeTableName);
            DataStorage.Tables.Add(table);

            var cols = table.Columns;

            cols.Add(new DataColumn(StringConst.FieldId, typeof (int)));
            cols.Add(new DataColumn(StringConst.FieldParentId, typeof (int)));
            cols.Add(new DataColumn(StringConst.FieldType, typeof (string)));
            cols.Add(new DataColumn(StringConst.FieldName, typeof (string)));
            cols.Add(new DataColumn(StringConst.FieldImage, typeof (int)));

            table = new DataTable(LeafTableName);
            DataStorage.Tables.Add(table);

            cols = table.Columns;

            cols.Add(new DataColumn(StringConst.FieldId, typeof (int)));
            cols.Add(new DataColumn(StringConst.FieldParentId, typeof (int)));
            cols.Add(new DataColumn(StringConst.FieldName, typeof (string)));
            cols.Add(new DataColumn(StringConst.FieldImage, typeof (int)));
            cols.Add(new DataColumn(StringConst.FieldDescription, typeof (string)));
            cols.Add(new DataColumn(StringConst.FieldVersion, typeof(string)));
        }

        protected static void LoadDataFromDb()
        {
            for (var i = 0; i < NodeDbTable.GetLength(0); i++)
            {
                var row = GetNodes().NewRow();
                row[0] = NodeDbTable[i, 0];
                row[1] = NodeDbTable[i, 1];
                row[2] = NodeDbTable[i, 2];
                row[3] = NodeDbTable[i, 3];
                row[4] = NodeDbTable[i, 4];
                GetNodes().Rows.Add(row);
            }

            for (var i = 0; i < LeafDbTable.GetLength(0); i++)
            {
                var row = GetLeafs().NewRow();
                row[0] = LeafDbTable[i, 0];
                row[1] = LeafDbTable[i, 1];
                row[2] = LeafDbTable[i, 2];
                row[3] = LeafDbTable[i, 3];
                row[4] = $"{LeafDbTable[i, 2]} Description.\nExists for: {LeafDbTable[i, 4]}\n.net Frameworks";
                row[5] = LeafDbTable[i, 4];
                GetLeafs().Rows.Add(row);
            }
        }

        public static void AddRecentLeaf(string projectType)
        {
            var rows = GetLeafs().Select($"{StringConst.FieldParentId} = {_recentId} AND {StringConst.FieldName} = '{projectType}'");

            if (rows.Any())
                return;

            var row = GetLeafs().NewRow();
            row[0] = _recentCounterId++;
            row[1] = _recentId;
            row[2] = projectType;
            row[3] = 1;
            row[4] = projectType + " Description";
            row[5] = "2.0;3.0;4.0;4.5";
            GetLeafs().Rows.Add(row);
        }
    }
}
