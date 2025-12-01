using Sea_Battle.ViewModels;
using System.Windows;

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
