using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using PropertyChanged;
using Xamarin.CommunityToolkit.UI.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Globalization;
using Bogus;

namespace TabTest
{
    public enum OrderStatus
    {
        ToBePicked = 0, Picking, Picked
    }

    [AddINotifyPropertyChangedInterface]
    public partial class MainPage : ContentPage
    {
        public List<TabHeader> Tabs { get; set; }
        public ObservableCollection<Order> Orders { get; set; }
        public int TotalOrders { get; set; }
        public int CurrentTabIndex { get; set; }
        public ICommand TabChangedCommand { get; set; }
        public bool IsRefreshing { get; set; }
        public ICommand RefreshCommand { get; set; }

        private List<Order> AllOrders;

        public MainPage()
        {
            InitializeComponent();

            Orders = new ObservableCollection<Order>();

            BindingContext = this;
            Tabs = new List<TabHeader>
            {
                new TabHeader { TabName = "To Be Picked", Count = 0, TabIndex = 0 },
                new TabHeader { TabName = "Picking", Count = 0, TabIndex = 1 },
                new TabHeader { TabName = "Picked", Count = 0, TabIndex = 2 },
            };
            DownloadOrders();

            TabChangedCommand = new Command<int>(TabSelectionChanged);
            RefreshCommand = new Command(async () => await RefreshOrders());
            CurrentTabIndex = 0;
        }

        void DownloadOrders()
        {
            Randomizer.Seed = new Random(8367363);

            string[] slots = new string[] { "08:00 - 12:00", "12:00 - 16:00", "16:00 - 20:00" };

            var bogusOrders = new Faker<Order>()
                .RuleFor(o => o.OrderId, f => f.Random.Number(10000, 50000))
                .RuleFor(o => o.CustomerName, f => f.Name.FullName())
                .RuleFor(o => o.Status, f => f.PickRandom<OrderStatus>())
                .RuleFor(o => o.Slot, f => f.PickRandom(slots))
                .RuleFor(o => o.Count, f => f.Random.Number(11, 44));

            AllOrders = new List<Order>(bogusOrders.Generate(50));
        }

        void TabSelectionChanged(int newTabPosition)
        {
            CurrentTabIndex = Tabs[newTabPosition].TabIndex;
            LoadOrders();
        }

        async Task RefreshOrders()
        {
            if (IsRefreshing) return;

            DownloadOrders();
            await Task.Delay(3000); // fake some delay from API
            LoadOrders();

            IsRefreshing = false;
        }

        void LoadOrders()
        {
            Orders = new ObservableCollection<Order>(AllOrders.Where(x => (int)x.Status == CurrentTabIndex));
            UpdateTabCounts();
        }

        void UpdateTabCounts()
        {
            // loop through and update counts in tabs
            foreach (var tab in Tabs)
            {
                tab.Count = AllOrders.Count(x => (int)x.Status == tab.TabIndex);
            }
            TotalOrders = AllOrders?.Count ?? 0;
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class TabHeader
    {
        public string TabName { get; set; }
        public int Count { get; set; }
        public int TabIndex { get; set; }
        public string TabTitle => $"{TabName} ({Count})";
    }

    [AddINotifyPropertyChangedInterface]
    public class Order
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public OrderStatus Status { get; set; }
        public string Slot { get; set; }
        public int Count { get; set; }
    }

    public class TabSelectionChangedEventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var eventArgs = value as TabSelectionChangedEventArgs;
            if (eventArgs == null)
            {
                throw new ArgumentException("Expected value to be of type TabSelectionChangedEventArgs", nameof(value));
            }
            return eventArgs.NewPosition;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
