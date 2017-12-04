using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Grid;
using Telerik.UI.Xaml.Controls.Grid.Primitives;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FirstUwpApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //public ObservableCollection<dynamic> People { get; set; }
        //public ObservableCollection<object> People1 { get; set; }

        private const string DataFilePath = "Data.txt";
        private const string ColumnNamePrefix = "ColumnName_";
        private const int EmptyRowToAddForNew = 3;

        public Dictionary<string, ObservableCollection<object>> DataSources = new Dictionary<string, ObservableCollection<object>>();

        public MainPage()
        {
            this.InitializeComponent();

            LoadDataOnUI();
            

            /*var jsonString = "{\"Items\":[{\"FruitName\":\"Apple\", \"Price\":12.3 },{\"FruitName\":\"Grape\", \"Price\":3.21 }],\"Date\":\"21/11/2010\"}";

            var myclass = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonString);

            this.People = new ObservableCollection<dynamic>();
            foreach (var p in myclass["Items"])
            {
                var exo = new ExpandoObject() as IDictionary<string, Object>;

                // object p = new object;
                foreach (JProperty jp in p.Properties())
                {
                    string x = jp.Name;
                    var y = jp.Value;
                    exo.Add(x, (string)y);
                    //p[x] = y;
                }
                this.People.Add(exo);
            }


            this.People1 = new ObservableCollection<object>
            {
                new { Name = "Y", Red = "Y"},
                new { Name = "X", Red = "Y"},
                new { Name = "Z", Red = "Y"}
            };*/
        }

        private ObservableCollection<dynamic> GetTableSourceObjectFromJson(string listName, string json)
        {
            var dataSourceObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);

            var source = new ObservableCollection<dynamic>();

            foreach (var p in dataSourceObject[listName])
            {
                var exo = new ExpandoObject() as IDictionary<string, Object>;

                foreach (JProperty jp in p.Properties())
                {
                    string x = jp.Name;
                    var y = jp.Value;
                    exo.Add(x, (string)y);
                }

                source.Add(exo);
            }

            return source;
        }

        private static void FindChildren<T>(List<T> results, DependencyObject startNode) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(startNode);

            for (int i = 0; i < count; i++)
            {
                DependencyObject current = VisualTreeHelper.GetChild(startNode, i);
                if ((current.GetType()).Equals(typeof(T)) || (current.GetType().GetTypeInfo().IsSubclassOf(typeof(T))))
                {
                    T asType = (T)current;
                    results.Add(asType);
                }
                FindChildren<T>(results, current);
            }
        }

        private async Task<string> ReadData()
        {
            //Create the text file to hold the data
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            string data = string.Empty;

            try
            {
                var file = await storageFolder.GetFileAsync(DataFilePath);
                data = await Windows.Storage.FileIO.ReadTextAsync(file);
            }
            catch
            {
                
            }

            if (string.IsNullOrWhiteSpace(data))
            {
                data = "{\"ListView_People\":[{\"Name\":\"Alex Tudor\",\"Age\":45,\"Country\":\"England\"},{\"Name\":\"Brian Murphy\",\"Age\":35,\"Country\":\"Zimbabwe\"},{\"Name\":\"Steve Johnson\",\"Age\":27,\"Country\":\"Australia\"},{\"Name\":\"Jonathon Singh\",\"Age\":23,\"Country\":\"Monaco\"},{\"Name\":\"Javagal Jasprit\",\"Age\":58,\"Country\":\"India\"},{\"Name\":\"Kane Hudson\",\"Age\":45,\"Country\":\"Canada\"}],\"Rad_people\":[{\"Name\":\"Alex Tudor\",\"Age\":45,\"Country\":\"England\"},{\"Name\":\"Brian Murphy\",\"Age\":35,\"Country\":\"Zimbabwe\"},{\"Name\":\"Steve Johnson\",\"Age\":27,\"Country\":\"Australia\"},{\"Name\":\"Jonathon Singh\",\"Age\":23,\"Country\":\"Monaco\"},{\"Name\":\"Javagal Jasprit\",\"Age\":58,\"Country\":\"India\"},{\"Name\":\"Kane Hudson\",\"Age\":45,\"Country\":\"Canada\"}],\"Rad_Fruit\":[{\"FruitName\":\"Apple\",\"Price\":12.3},{\"FruitName\":\"Orange\",\"Price\":21.2},{\"FruitName\":\"Grape\",\"Price\":3.21}],\"ListView_Fruit\":[{\"FruitName\":\"Apple\",\"Price\":12.3},{\"FruitName\":\"Orange\",\"Price\":21.2},{\"FruitName\":\"Grape\",\"Price\":3.21}]}";
            }

            return data;
        }

        private async Task WriteData(string data)
        {
            try
            {
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile file = await storageFolder.CreateFileAsync(DataFilePath, Windows.Storage.CreationCollisionOption.ReplaceExisting);


                await Windows.Storage.FileIO.WriteTextAsync(file, data);
            }
            catch (Exception ex)
            {
                var x = ex.Message;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string jsonToSave = Newtonsoft.Json.JsonConvert.SerializeObject(DataSources);
            await WriteData(jsonToSave);
        }

        private List<string> FindListViewColumnNames(ListView listView)
        {
            List<string> columnsNames = new List<string>();

            //Find All textblocks first
            List<TextBlock> blocks = new List<TextBlock>();
            FindChildren(blocks, listView);

            foreach(var textBlock in blocks)
            {
                if(textBlock.Name.StartsWith(ColumnNamePrefix))
                {
                    columnsNames.Add(textBlock.Text.Replace(" ", string.Empty));
                }
            }

            return columnsNames;
        }

        private List<string> FindRadGridColumnNames(RadDataGrid grid)
        {
            List<string> columnsNames = new List<string>();

            List<DataGridColumnHeader> columnHeaders = new List<DataGridColumnHeader>();
            FindChildren<DataGridColumnHeader>(columnHeaders, grid);

            foreach (DataGridColumnHeader columnHeader in columnHeaders)
            {
                columnsNames.Add(columnHeader.Content.ToString().Replace(" ", string.Empty));
            }

            return columnsNames;
        }

        private ObservableCollection<dynamic> PrepareEmptySourceForTable(List<string> columnNames)
        {
            var source = new ObservableCollection<dynamic>();

            for (var count = 0; count < EmptyRowToAddForNew; count++)
            {
                var exo = new ExpandoObject() as IDictionary<string, Object>;

                foreach (var columnName in columnNames)
                {
                    exo.Add(columnName, "");
                }

                source.Add(exo);
            }

            return source;
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            this.DataSources.Clear();

            //Find listviews
            var allListViews = new List<ListView>();
            FindChildren(allListViews, this);


            foreach (var grid in allListViews)
            {
                var columnNames = FindListViewColumnNames(grid);

                var source = PrepareEmptySourceForTable(columnNames);

                grid.ItemsSource = source;

                this.DataSources.Add(grid.Name, source);
            }

            //FindName RadDataGrids
            var allRadGrids = new List<RadDataGrid>();
            FindChildren(allRadGrids, this);

            foreach (var grid in allRadGrids)
            {
                var columnNames = FindRadGridColumnNames(grid);

                var source = PrepareEmptySourceForTable(columnNames);

                grid.ItemsSource = source;

                this.DataSources.Add(grid.Name, source);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDataOnUI();
        }

        private async void LoadDataOnUI()
        {
            this.DataSources.Clear();
            string dataSourceJson = await ReadData();
            //if (File.Exists(DataFilePath))
            //{
            //    dataSourceJson = File.ReadAllText(DataFilePath);
            //}

            //if (string.IsNullOrEmpty(dataSourceJson))
            //{
            //    dataSourceJson = "{\"ListView_People\":[{\"Name\":\"Alex Tudor\",\"Age\":45,\"Country\":\"England\"},{\"Name\":\"Brian Murphy\",\"Age\":35,\"Country\":\"Zimbabwe\"},{\"Name\":\"Steve Johnson\",\"Age\":27,\"Country\":\"Australia\"},{\"Name\":\"Jonathon Singh\",\"Age\":23,\"Country\":\"Monaco\"},{\"Name\":\"Javagal Jasprit\",\"Age\":58,\"Country\":\"India\"},{\"Name\":\"Kane Hudson\",\"Age\":45,\"Country\":\"Canada\"}],\"Rad_people\":[{\"Name\":\"Alex Tudor\",\"Age\":45,\"Country\":\"England\"},{\"Name\":\"Brian Murphy\",\"Age\":35,\"Country\":\"Zimbabwe\"},{\"Name\":\"Steve Johnson\",\"Age\":27,\"Country\":\"Australia\"},{\"Name\":\"Jonathon Singh\",\"Age\":23,\"Country\":\"Monaco\"},{\"Name\":\"Javagal Jasprit\",\"Age\":58,\"Country\":\"India\"},{\"Name\":\"Kane Hudson\",\"Age\":45,\"Country\":\"Canada\"}],\"Rad_Fruit\":[{\"FruitName\":\"Apple\",\"Price\":12.3},{\"FruitName\":\"Orange\",\"Price\":21.2},{\"FruitName\":\"Grape\",\"Price\":3.21}],\"ListView_Fruit\":[{\"FruitName\":\"Apple\",\"Price\":12.3},{\"FruitName\":\"Orange\",\"Price\":21.2},{\"FruitName\":\"Grape\",\"Price\":3.21}]}";
            //}

            //Find listviews
            var allListViews = new List<ListView>();
            FindChildren(allListViews, this);

            foreach (var grid in allListViews)
            {
                var source = GetTableSourceObjectFromJson(grid.Name, dataSourceJson);
                
                this.DataSources.Add(grid.Name, source);

                grid.ItemsSource = source;
            }

            //FindName RadDataGrids
            var allRadGrids = new List<RadDataGrid>();
            FindChildren(allRadGrids, this);

            foreach (var grid in allRadGrids)
            {
                var source = GetTableSourceObjectFromJson(grid.Name, dataSourceJson);

                this.DataSources.Add(grid.Name, source);

                grid.ItemsSource = source;
            }
        }
    }
}
