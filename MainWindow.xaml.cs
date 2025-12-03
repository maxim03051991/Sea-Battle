using Sea_Battle.ViewModels;
using System.Windows;

namespace Sea_Battle
{
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
        //выбор классической игры
        private void ClassicGame_Click(object sender, RoutedEventArgs e)
        {
            StartGame(false);
        }
        //выбор игры по новым правилам
        private void NewRulesGame_Click(object sender, RoutedEventArgs e)
        {
            StartGame(true);
        }
        //начало игры и постоение экрана
        private void StartGame(bool isNewRulesMode)
        {
            _isNewRulesMode = isNewRulesMode;
            DataContext = new GameViewModel(_isNewRulesMode);

            // Переключаемся на игровой экран
            StartupScreen.Visibility = Visibility.Collapsed;
            GameScreen.Visibility = Visibility.Visible;
        }
        //экран выбора режима игры
        private void ShowStartupScreen()
        {
            StartupScreen.Visibility = Visibility.Visible;
            GameScreen.Visibility = Visibility.Collapsed;
        }
    }
}
