using System.Windows.Controls;
using System.Windows.Input;

namespace BSACLibrary
{
    /// <summary>
    /// Задаем поведение для элемента comboBox
    /// </summary>
    public class CustomComboBox : ComboBox
    {
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) && IsDropDownOpen && (SelectedIndex == -1))
            {
                IsDropDownOpen = false;
                base.OnPreviewKeyDown(e);
            }
            else if ((e.Key == Key.Enter) && IsDropDownOpen && (SelectedIndex > -1))
            {
                e.Handled = true;
                SelectedIndex = -1;
                //При нажатии на элемент списка вызываем файл
                //Список не сворачивается
                //Новый поиск не инициализируется
            }
            else
            {
                SelectedIndex = -1;
                base.OnPreviewKeyDown(e);
            }
        }
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            //Позволяет не вставлять в поле ввода выделенный элемент
        }
    }
}
