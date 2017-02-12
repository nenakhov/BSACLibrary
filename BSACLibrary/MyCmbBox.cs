using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BSACLibrary
{
    /// <summary>
    /// Задаем поведение для элемента comboBox
    /// </summary>
    public class CustomComboBox : ComboBox
    {
        //protected override void OnPreviewKeyDown(KeyEventArgs e)
        //{
        //    if ((e.Key == Key.Enter) && (IsDropDownOpen = true) && (SelectedIndex == -1))
        //    {
        //        IsDropDownOpen = false;
        //        base.OnPreviewKeyDown(e);
        //    }
        //    else if ((e.Key == Key.Enter) && (IsDropDownOpen = true) && (SelectedIndex > -1))
        //    {
        //        SelectedIndex = -1;
        //        e.Handled = true;
        //        //При нажатии на элемент списка вызываем файл
        //        //Список не сворачивается
        //        //Новый поиск не инициализируется
        //    }
        //    else
        //    {
        //        SelectedIndex = -1;
        //        base.OnPreviewKeyDown(e);
        //    }
        //}
        //protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        //{
        //    //KeyboardDevice keyboardDevice = Keyboard.PrimaryDevice;
        //    //PresentationSource presentationSource = Keyboard.PrimaryDevice.ActiveSource;
        //    //KeyEventArgs keyEventArgs = new KeyEventArgs(keyboardDevice, presentationSource, 0, Key.Enter);
        //    //OnPreviewKeyDown(keyEventArgs);
        //    //if (Mouse.LeftButton == MouseButtonState.Released)
        //    //{
        //    //    KeyboardDevice keyboardDevice = Keyboard.PrimaryDevice;
        //    //    PresentationSource presentationSource = Keyboard.PrimaryDevice.ActiveSource;
        //    //    KeyEventArgs keyEventArgs = new KeyEventArgs(keyboardDevice, presentationSource, 0, Key.Enter);
        //    //    OnPreviewKeyDown(keyEventArgs);
        //    //    IsDropDownOpen = true;
        //    //}
        //    //Позволяет не вставлять в поле ввода выделенный элемент
        //}
    }
}
