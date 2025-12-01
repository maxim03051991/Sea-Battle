using System.Windows.Input;

namespace Sea_Battle.ViewModels
{ //реализация паттерна RelayCommand
    public class RelayCommand : ICommand
    {
        // Поле для хранения действия, которое выполняется при вызове команды
        private readonly Action _execute;
        // Поле для хранения функции, проверяющей возможность выполнения команды
        private readonly Func<bool> _canExecute;

        // Конструктор принимает действие и опциональную функцию проверки
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            // Проверка, что действие не null, иначе исключение
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        // Метод проверки возможности выполнения команды
        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        // Метод выполнения команды
        public void Execute(object parameter) => _execute();
        // Событие изменения возможности выполнения
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public class RelayCommand<T> : ICommand
    {
        // Поле для хранения действия с параметром типа T
        private readonly Action<T> _execute;
        // Поле для хранения функции проверки с параметром типа T
        private readonly Func<T, bool> _canExecute;

        // Конструктор для команд с параметром
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        // Проверка возможности выполнения с приведением типа параметра
        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;
        // Выполнение команды с приведением типа параметра
        public void Execute(object parameter) => _execute((T)parameter);
        // Событие изменения возможности выполнения
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
