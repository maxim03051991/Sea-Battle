using Sea_Battle.model;
using Sea_Battle.services;
using Sea_Battle.ViewModels;
using Sea_Battle.Converters;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sea_Battle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isNewRulesMode;

        public MainWindow()
        {
            InitializeComponent();
            // По умолчанию показываем экран выбора режима
            ShowStartupScreen();
        }

        public MainWindow(bool isNewRulesMode)
        {
            _isNewRulesMode = isNewRulesMode;
            InitializeComponent();

            // Если режим передан параметром, сразу запускаем игру
            if (isNewRulesMode)
            {
                StartGame(true);
            }
            else
            {
                ShowStartupScreen();
            }
        }

        private void ClassicGame_Click(object sender, RoutedEventArgs e)
        {
            StartGame(false);
        }

        private void NewRulesGame_Click(object sender, RoutedEventArgs e)
        {
            StartGame(true);
        }

        private void StartGame(bool isNewRulesMode)
        {
            _isNewRulesMode = isNewRulesMode;
            DataContext = new GameViewModel(_isNewRulesMode);

            // Переключаемся на игровой экран
            StartupScreen.Visibility = Visibility.Collapsed;
            GameScreen.Visibility = Visibility.Visible;
        }

        private void ShowStartupScreen()
        {
            StartupScreen.Visibility = Visibility.Visible;
            GameScreen.Visibility = Visibility.Collapsed;
        }
    }
}
